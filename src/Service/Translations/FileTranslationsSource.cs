using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Karambolo.PO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WebApp.Core.Infrastructure;
using WebApp.Service.Helpers;
using WebApp.Service.Infrastructure.Events;

namespace WebApp.Service.Translations
{
    internal sealed class FileTranslationsSource : ITranslationsSource, IDisposable
    {
        private const string FileNamePattern = "*.po";

        private static readonly POParserSettings s_parserSettings = new POParserSettings
        {
            SkipComments = true,
            SkipInfoHeaders = true,
        };

        private readonly IEventNotifier _eventNotifier;
        private readonly IClock _clock;
        private readonly ILogger _logger;

        private readonly string _translationsBasePath;
        private readonly bool _reloadOnChange;
        private readonly TimeSpan _reloadDelay;
        private readonly TimeSpan _delayOnWatcherError;

        private readonly Dictionary<(string Location, string Culture), FileInfo> _files;
        private readonly TaskCompletionSource<object?> _initializedTcs;
        private readonly IDisposable _notifySubscription;

        private Exception? _previousObtainFilesException;

        public FileTranslationsSource(IEventNotifier eventNotifier, IClock clock, IOptions<FileTranslationsSourceOptions>? options, ILogger<FileTranslationsSource>? logger)
        {
            _eventNotifier = eventNotifier ?? throw new ArgumentNullException(nameof(eventNotifier));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? (ILogger)NullLogger.Instance;

            var optionsValue = options?.Value;
            _translationsBasePath = optionsValue?.TranslationsBasePath ?? Path.Combine(AppContext.BaseDirectory, "Translations");
            _reloadOnChange = optionsValue?.ReloadOnChange ?? FileTranslationsSourceOptions.DefaultReloadOnChange;
            _reloadDelay = optionsValue?.ReloadDelay ?? FileTranslationsSourceOptions.DefaultReloadDelay;
            _delayOnWatcherError = optionsValue?.DelayOnWatcherError ?? FileTranslationsSourceOptions.DefaultDelayOnWatcherError;

            _files = new Dictionary<(string, string), FileInfo>();

            _initializedTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            var loadFiles = PollFiles()
                .Catch<(string[] filePaths, Exception? exception), DirectoryNotFoundException>(_ => Observable.Empty<(string[], Exception?)>())
                .SelectMany(item => item.filePaths.Select(filePath => (filePath, item.exception)).ToObservable())
                .SelectMany(item => LoadFile(item.filePath, item.exception)
                    .Catch<(string, TranslationsChangedEvent), Exception>(ex => Observable.Empty<(string, TranslationsChangedEvent)>()))
                .Do(Noop<(string, TranslationsChangedEvent)>.Action, ex => _initializedTcs.TrySetException(ex), () => _initializedTcs.TrySetResult(null));

            if (_reloadOnChange)
            {
                loadFiles = loadFiles.Concat(WatchFiles()
                    .Retry<(string[] filePaths, Exception? exception)>(wrapSubsequent: source => PollFiles()
                        .Catch<(string[], Exception?), Exception>(ex => Observable
                            .Return<(string[], Exception?)>((GetCurrentFiles(), ex))
                            .Concat(Observable.Throw<(string[], Exception?)>(ex)))
                        .Concat(source)
                        .DelaySubscription(_delayOnWatcherError))
                    .SelectMany(item => item.filePaths.Select(filePath => (filePath, item.exception)).ToObservable())
                    .GroupBy(item => item.filePath)
                    .SelectMany(group => group
                        .Throttle(_reloadDelay)
                        .Select(item => LoadFile(item.filePath, item.exception)
                            .Catch<(string, TranslationsChangedEvent), Exception>(ex => Observable.Empty<(string, TranslationsChangedEvent)>()))
                        .Switch()));
            }

            _notifySubscription = loadFiles
                .Subscribe(item =>
                {
                    var (filePath, @event) = item;
                    if (RegisterChange(filePath, @event))
                    {
                        _logger.LogInformation("Translation file \"{PATH}\" has been loaded.", filePath);
                        _eventNotifier.Notify(@event);
                    }
                });
        }

        public void Dispose()
        {
            _notifySubscription.Dispose();
        }

        private IObservable<(string[], Exception?)> PollFiles() => Observable
            .Defer<string[]>(() => Observable.Return(Directory.GetFiles(_translationsBasePath, FileNamePattern, SearchOption.AllDirectories)))
            .Do(OnObtainFilesSuccess, OnObtainFilesError)
            .Select(filePaths => (filePaths, (Exception?)null));

        private IObservable<(string[], Exception?)> WatchFiles() => Observable
            .Create<string[]>(observer =>
            {
                FileSystemWatcher watcher;
                try { watcher = new FileSystemWatcher(_translationsBasePath, FileNamePattern); }
                catch (ArgumentException ex)
                {
                    observer.OnError(new DirectoryNotFoundException(null, ex));
                    return Disposable.Empty;
                }

                watcher.IncludeSubdirectories = true;

                watcher.Error += (_, e) => observer.OnError(e.GetException());

                watcher.Created += (_, e) => observer.OnNext(new[] { e.FullPath });
                watcher.Changed += (_, e) => observer.OnNext(new[] { e.FullPath });
                watcher.Renamed += (_, e) => observer.OnNext(new[] { e.OldFullPath, e.FullPath });
                watcher.Deleted += (_, e) => observer.OnNext(new[] { e.FullPath });

                watcher.EnableRaisingEvents = true;

                return watcher;
            })
            .Do(OnObtainFilesSuccess, OnObtainFilesError)
            .Select(filePaths => (filePaths, (Exception?)null));

        private void ClearObtainFilesException() => Volatile.Write(ref _previousObtainFilesException, null);

        private void OnObtainFilesSuccess(string[] _) => ClearObtainFilesException();

        private void OnObtainFilesError(Exception ex)
        {
            // basic protection against littering the log with identical, recurring exceptions (e.g. file system errors, etc.)
            var previousException = Interlocked.Exchange(ref _previousObtainFilesException, ex);
            if (ex is DirectoryNotFoundException)
            {
                if (!(previousException is DirectoryNotFoundException))
                    _logger.LogWarning("Directory of translations at \"{PATH}\" does not exist. Translations will not be available.", _translationsBasePath);
            }
            else if (_previousObtainFilesException?.ToString() != ex.ToString())
                _logger.LogError(ex, "Obtaining list of translation files failed.");
        }

        private async Task<TranslationCatalogData?> LoadTranslationsAsync(string filePath, CancellationToken cancellationToken)
        {
            FileStream fileStream;
            try { fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true); }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException) { return null; }

            POParseResult parseResult;
            using (fileStream)
            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                memoryStream.Position = 0;
                parseResult = new POParser(s_parserSettings).Parse(memoryStream);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!parseResult.Success)
            {
                var diagnosticMessages = parseResult.Diagnostics
                    .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

                _logger.LogWarning("Translation file \"{PATH}\" has errors: {ERRORS}", filePath, string.Join(Environment.NewLine, diagnosticMessages));
                return null;
            }

            return parseResult.Catalog.ToData();
        }

        // we force loading to the thread pool using Task.Run to not block the FileSystemWatcher's callback threads by any means
        // because it might lead to missed events and/or buffer overflow
        // https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?redirectedfrom=MSDN&view=netcore-3.1#events-and-buffer-sizes
        private Task<(string FilePath, TranslationsChangedEvent? Event)> LoadFileAsync(string filePath, Exception? obtainFilesException, CancellationToken cancellationToken) => Task.Run(async () =>
        {
            var relativeFilePath = Path.GetRelativePath(_translationsBasePath, filePath);

            var culture = Path.GetDirectoryName(relativeFilePath);
            if (string.IsNullOrEmpty(culture) || !string.IsNullOrEmpty(Path.GetDirectoryName(culture)))
                return (filePath, (TranslationsChangedEvent?)null);

            var location = Path.GetFileNameWithoutExtension(relativeFilePath);
            return (filePath, new TranslationsChangedEvent
            {
                Version = _clock.TimestampTicks,
                Location = location,
                Culture = culture,
                Data =
                    obtainFilesException == null ?
                    await LoadTranslationsAsync(filePath, cancellationToken).ConfigureAwait(false) :
                    null
            });
        }, cancellationToken);

        private IObservable<(string, TranslationsChangedEvent)> LoadFile(string filePath, Exception? obtainFilesException) => Observable
            .FromAsync(ct => LoadFileAsync(filePath, obtainFilesException, ct))
            .Where(item => item.Event != null)!
            .Do(Noop<(string, TranslationsChangedEvent)>.Action, ex => _logger.LogError(ex, "Unexpected error occurred when loading translation file \"{PATH}\".", filePath));

        private string[] GetCurrentFiles()
        {
            lock (_files)
                return _files.Values.Select(file => file.FilePath).ToArray();
        }

        private bool RegisterChange(string filePath, TranslationsChangedEvent @event)
        {
            var key = (@event.Location, @event.Culture);
            lock (_files)
            {
                if (!_files.TryGetValue(key, out var file))
                    _files.Add(key, file = new FileInfo());
                else if (file.LastEvent.Version >= @event.Version || file.LastEvent.Data == null && @event.Data == null)
                    return false;

                file.FilePath = filePath;
                file.LastEvent = @event;
                return true;
            }
        }

        public async Task<TranslationsChangedEvent[]> GetLatestVersionAsync(CancellationToken cancellationToken)
        {
            if (!_initializedTcs.Task.IsCompleted)
                await _initializedTcs.Task.AsCancelable(cancellationToken).ConfigureAwait(false);

            lock (_files)
                return _files.Values.Select(file => file.LastEvent).ToArray();
        }

        public void Invalidate(string? location, string? culture) => throw new NotSupportedException();

        private sealed class FileInfo
        {
            public string FilePath { get; set; } = null!;
            public TranslationsChangedEvent LastEvent { get; set; } = null!;
        }
    }
}
