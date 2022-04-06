using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using MimeKit;
using WebApp.Core.Helpers;
using WebApp.Core.Infrastructure;
using WebApp.DataAccess;
using WebApp.DataAccess.Entities;
using WebApp.Service.Helpers;

namespace WebApp.Service.Mailing
{
    internal sealed class MailSenderService : BackgroundService, IMailSenderService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMailTypeCatalog _mailTypeCatalog;
        private readonly IClock _clock;

        private readonly string? _smtpHost;
        private readonly int _smtpPort;
        private readonly SecureSocketOptions _smtpSecurity;
        private readonly NetworkCredential? _smtpCredentials;

        private readonly Func<MailTransport> _smtpClientFactory;
        private MailTransport? _smtpClient;

        private readonly ushort _batchSize;
        private readonly TimeSpan _maxSleepTime;
        private readonly TimeSpan _initialSendRetryTime;
        private readonly TimeSpan _maxSendRetryTime;
        private readonly TimeSpan _delayOnUnexpectedError;

        private readonly ILogger _logger;

        public MailSenderService(IServiceScopeFactory serviceScopeFactory, IMailTypeCatalog mailTypeCatalog, IGuidProvider guidProvider, IClock clock,
            IOptions<SmtpOptions>? smtpOptions, IOptions<MailSenderServiceOptions>? options, ILogger<MailSenderService>? logger)
        {
            if (guidProvider == null)
                throw new ArgumentNullException(nameof(guidProvider));

            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _mailTypeCatalog = mailTypeCatalog ?? throw new ArgumentNullException(nameof(mailTypeCatalog));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            var smtpOptionsValue = smtpOptions?.Value;
            var usePickupDir = smtpOptionsValue?.UsePickupDir ?? true;
            if (usePickupDir)
            {
                var pickupDirPath = smtpOptionsValue?.PickupDirPath ?? string.Empty;
                if (!Path.IsPathRooted(pickupDirPath))
                    pickupDirPath = Path.Combine(AppContext.BaseDirectory, pickupDirPath);

                _smtpClientFactory = () => new PickupDirMailClient(guidProvider, pickupDirPath);
            }
            else
            {
                _smtpHost =
                    smtpOptionsValue!.Host ??
                    throw new ArgumentException($"{nameof(SmtpOptions.Host)} must be specified.", nameof(smtpOptions));

                _smtpPort = smtpOptionsValue.Port ?? SmtpOptions.DefaultPort;
                _smtpSecurity = smtpOptionsValue.Security ?? SmtpOptions.DefaultSecurity;

                var smtpUserName = smtpOptionsValue.UserName;
                var smtpPassword = smtpOptionsValue.Password;
                _smtpCredentials = smtpUserName != null || smtpPassword != null ? new NetworkCredential(smtpUserName, smtpPassword) : null;

                var smtpTimeout = smtpOptionsValue.Timeout ?? SmtpOptions.DefaultTimeout;
                _smtpClientFactory = () => new SmtpClient { Timeout = checked((int)smtpTimeout.TotalMilliseconds) };
            }

            var optionsValue = options?.Value;
            _batchSize = optionsValue?.BatchSize ?? MailSenderServiceOptions.DefaultBatchSize;

            _maxSleepTime = optionsValue?.MaxSleepTime ?? MailSenderServiceOptions.DefaultMaxSleepTime;

            _initialSendRetryTime = optionsValue?.InitialSendRetryTime ?? _maxSleepTime;
            if (_initialSendRetryTime <= TimeSpan.Zero)
                _initialSendRetryTime = MailSenderServiceOptions.DefaultInitialSendRetryTime;

            _maxSendRetryTime = optionsValue?.MaxSendRetryTime ?? MailSenderServiceOptions.DefaultMaxSendRetryTime;
            if (_maxSendRetryTime < _initialSendRetryTime)
                _maxSendRetryTime = _initialSendRetryTime;

            _delayOnUnexpectedError = optionsValue?.DelayOnUnexpectedError ?? MailSenderServiceOptions.DefaultDelayOnUnexpectedError;

            _logger = logger ?? (ILogger)NullLogger.Instance;
        }

        private event EventHandler? Enqueued;

        private void WakeProcessor() => Enqueued?.Invoke(this, EventArgs.Empty);

        public async Task EnqueueItemAsync(MailModel model, WritableDataContext dbContext, IChangeToken? transactionCommittedToken, CancellationToken cancellationToken)
        {
            var hasPendingTransaction = dbContext.Database.HasPendingTransaction();
            if (hasPendingTransaction && transactionCommittedToken == null)
                throw new ArgumentNullException(nameof(transactionCommittedToken), "A change token signalling on successful commit must be supplied when a transaction is present.");

            var mailTypeDefinition = _mailTypeCatalog.GetDefinition(model.MailType, throwIfNotFound: true)!;

            var serializedMailModel = mailTypeDefinition.SerializeModel(model);

            var utcNow = _clock.UtcNow;
            dbContext.MailQueue.Add(new MailQueueItem
            {
                CreationDate = utcNow,
                DueDate = utcNow,
                MailType = model.MailType,
                MailModel = serializedMailModel,
            });

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (hasPendingTransaction)
                transactionCommittedToken!.RegisterChangeCallback(state => ((MailSenderService)state).WakeProcessor(), this);
            else
                WakeProcessor();
        }

        private IAsyncEnumerable<MailQueueItem> PeekItems(WritableDataContext dbContext)
        {
            var utcNow = _clock.UtcNow;

            IQueryable<MailQueueItem> queueItems =
                from item in dbContext.MailQueue
                where item.DueDate != null && item.DueDate.Value <= utcNow
                orderby item.DueDate
                select item;

            if (_batchSize > 0)
                queueItems = queueItems.Take(_batchSize);

            return queueItems.AsAsyncEnumerable();
        }

        private async Task RemoveItemAsync(Mail mail, WritableDataContext dbContext, CancellationToken cancellationToken)
        {
            dbContext.MailQueue.Remove(mail.QueueItem);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private DateTime GetRetryDueDate(DateTime creationDate, DateTime currentDueDate)
        {
            var retryTime = currentDueDate - creationDate;
            if (retryTime < TimeSpan.Zero)
                retryTime = TimeSpan.Zero;

            retryTime += _initialSendRetryTime;
            if (retryTime > _maxSendRetryTime)
                retryTime = _maxSendRetryTime;

            return currentDueDate + retryTime;
        }

        private async Task RegisterItemFailureAsync(MailQueueItem queueItem, bool canRetry, WritableDataContext dbContext, CancellationToken cancellationToken)
        {
            queueItem.DueDate = canRetry ? GetRetryDueDate(queueItem.CreationDate, queueItem.DueDate!.Value) : (DateTime?)null;

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<Mail> ProduceMailAsync(MailQueueItem queueItem, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            IMailTypeDefinition mailTypeDefinition;
            try
            {
                mailTypeDefinition = _mailTypeCatalog.GetDefinition(queueItem.MailType, throwIfNotFound: true)!;
            }
            catch (ArgumentException ex)
            {
                return new Mail(queueItem, null, new MailTypeNotSupportedException(ex));
            }

            MailModel mailModel;
            try
            {
                mailModel = mailTypeDefinition.DeserializeModel(queueItem.MailModel);
            }
            catch (Exception ex)
            {
                return new Mail(queueItem, null, new MailModelSerializationException(ex));
            }

            var mailMessageProducer = mailTypeDefinition.CreateMailMessageProducer(serviceProvider);

            MimeMessage message;
            try
            {
                message = await mailMessageProducer.ProduceAsync(mailModel, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return new Mail(queueItem, null, ex);
            }

            return new Mail(queueItem, message, null);
        }

        private async Task<IReadOnlyList<Task<Mail>>> ProduceMailsAsync(IAsyncEnumerable<MailQueueItem> queueItems, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            List<Task<Mail>> produceMailTasks;

            var enumerator = queueItems.WithCancellation(cancellationToken).ConfigureAwait(false).GetAsyncEnumerator();
            try
            {
                if (!await enumerator.MoveNextAsync())
                    return Array.Empty<Task<Mail>>();

                produceMailTasks = new List<Task<Mail>>(1);

                ConfiguredValueTaskAwaitable<bool> moveNextTask;
                do
                {
                    var queueItem = enumerator.Current;
                    // ProduceMailAsync does some synchronous job before going async (e.g. deserialization of the model data),
                    // so before we're processing the current item, we instantly start to load the next one
                    moveNextTask = enumerator.MoveNextAsync();

                    produceMailTasks.Add(ProduceMailAsync(queueItem, serviceProvider, cancellationToken));
                }
                while (await moveNextTask);
            }
            finally { await enumerator.DisposeAsync(); }

            await Task.WhenAll(produceMailTasks).ConfigureAwait(false);

            return produceMailTasks;
        }

        private async Task HandleMailErrorAsync(Mail mail, WritableDataContext dbContext, CancellationToken cancellationToken)
        {
            switch (mail.Error)
            {
                case MailTypeNotSupportedException ex:
                    _logger.LogError(ex.InnerException, "Mail type '{TYPE}' is not supported. Queue item id: {ID}.", mail.QueueItem.MailType, mail.QueueItem.Id);
                    await RegisterItemFailureAsync(mail.QueueItem, canRetry: false, dbContext, cancellationToken).ConfigureAwait(false);
                    return;
                case MailModelSerializationException ex:
                    _logger.LogError(ex.InnerException, "Model of mail type '{TYPE}' could not be deserialized. Queue item id: {ID}.", mail.QueueItem.MailType, mail.QueueItem.Id);
                    await RegisterItemFailureAsync(mail.QueueItem, canRetry: false, dbContext, cancellationToken).ConfigureAwait(false);
                    return;
                default:
                    _logger.LogError(mail.Error, "Producing MIME message of mail type '{TYPE}' failed. Queue item id: {ID}.", mail.QueueItem.MailType, mail.QueueItem.Id);
                    await RegisterItemFailureAsync(mail.QueueItem, canRetry: false, dbContext, cancellationToken).ConfigureAwait(false);
                    return;
            }
        }

        private async Task SendMailAsync(Mail mail, WritableDataContext dbContext, CancellationToken cancellationToken)
        {
            try
            {
                await _smtpClient!.SendAsync(mail.Message!, cancellationToken).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex.InnerException, "Sending MIME message of mail type '{TYPE}' failed. Queue item id: {ID}.", mail.QueueItem.MailType, mail.QueueItem.Id);
                await RegisterItemFailureAsync(mail.QueueItem, canRetry: false, dbContext, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (CommandException ex)
            {
                _logger.LogError(ex.InnerException, "Sending MIME message of mail type '{TYPE}' failed. Queue item id: {ID}.", mail.QueueItem.MailType, mail.QueueItem.Id);
                await RegisterItemFailureAsync(mail.QueueItem, canRetry: true, dbContext, cancellationToken).ConfigureAwait(false);
                return;
            }

            // cancellation token is not passed since mails should be sent and deleted without interruption,
            // otherwise they could be sent multiple times
            await RemoveItemAsync(mail, dbContext, default).ConfigureAwait(false);
        }

        private async Task SendMailsAsync(IReadOnlyList<Task<Mail>> produceMailTasks, WritableDataContext dbContext, CancellationToken cancellationToken)
        {
            try
            {
                for (int i = 0, n = produceMailTasks.Count; i < n; i++)
                {
                    var mail = await produceMailTasks[i].ConfigureAwait(false);

                    if (mail.Message != null)
                    {
                        if (!_smtpClient!.IsConnected)
                        {
                            try
                            {
                                await _smtpClient.ConnectAsync(_smtpHost, _smtpPort, _smtpSecurity, cancellationToken).ConfigureAwait(false);
                            }
                            catch (Exception ex) when (ex is not OperationCanceledException)
                            {
                                var smtpClient = _smtpClient;
                                _smtpClient = _smtpClientFactory();
                                smtpClient.Dispose();
                                throw;
                            }

                            if (_smtpCredentials != null)
                                await _smtpClient.AuthenticateAsync(_smtpCredentials, cancellationToken).ConfigureAwait(false);
                        }

                        await SendMailAsync(mail, dbContext, cancellationToken).ConfigureAwait(false);
                    }
                    else if (mail.Error != null)
                        await HandleMailErrorAsync(mail, dbContext, cancellationToken).ConfigureAwait(false);
                    else
                        // we should never get here
                        throw new InvalidOperationException();
                }
            }
            finally
            {
                if (_smtpClient!.IsConnected)
                    await _smtpClient.DisconnectAsync(quit: true, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await using (AsyncDisposableAdapter.From(_serviceScopeFactory.CreateScope(), out var scope).ConfigureAwait(false))
            await using (scope.ServiceProvider.GetRequiredService<IDbContextFactory<WritableDataContext>>().CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                IReadOnlyList<Task<Mail>> produceMailTasks;
                do
                {
                    var queueItems = PeekItems(dbContext);

                    produceMailTasks = await ProduceMailsAsync(queueItems, scope.ServiceProvider, cancellationToken).ConfigureAwait(false);

                    if (produceMailTasks.Count > 0)
                        await SendMailsAsync(produceMailTasks, dbContext, cancellationToken).ConfigureAwait(false);
                    else
                        return;
                }
                while (_batchSize > 0 && produceMailTasks.Count >= _batchSize);
            }
        }

        // https://blog.stephencleary.com/2020/05/backgroundservice-gotcha-startup.html
        // TODO: revise this workaround when upgrading to .NET 7+ (https://github.com/dotnet/runtime/issues/36063)
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            using (_smtpClient = _smtpClientFactory())
            // TODO: revise this approach when https://github.com/dotnet/runtime/issues/35962 gets resolved
            using (var wakeEvent = new AutoResetEvent(false))
            using (Observable
                .FromEventPattern(handler => Enqueued += handler, handler => Enqueued -= handler)
                .Subscribe(_ =>
                {
                    try { wakeEvent.Set(); }
                    catch (ObjectDisposedException) { /* ObjectDisposedException can safely be swallowed here */ }
                }))
            {
                Exception? previousException = null;
                for (; ; )
                    try
                    {
                        await ProcessAsync(stoppingToken).ConfigureAwait(false);

                        // when queue gets empty, we wait for wake event (firing when an item is added to the queue),
                        // but for maximum safety we check the queue periodically anyway
                        await wakeEvent.WaitAsync(_maxSleepTime, stoppingToken).ConfigureAwait(false);

                        previousException = null;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        // basic protection against littering the log with identical, recurring exceptions (e.g. db connection errors, etc.)
                        if (previousException?.ToString() != ex.ToString())
                            _logger.LogError(ex, "Unexpected error occurred in the mail sender background service.");

                        previousException = ex;

                        await Task.Delay(_delayOnUnexpectedError).ConfigureAwait(false);
                    }
            }
        }, stoppingToken);

        private sealed class MailTypeNotSupportedException : ApplicationException
        {
            public MailTypeNotSupportedException(Exception innerException) : base(null, innerException) { }
        }

        private sealed class MailModelSerializationException : ApplicationException
        {
            public MailModelSerializationException(Exception innerException) : base(null, innerException) { }
        }

        private readonly struct Mail
        {
            public Mail(MailQueueItem queueItem, MimeMessage? message, Exception? error)
            {
                QueueItem = queueItem;
                Message = message;
                Error = error;
            }

            public MailQueueItem QueueItem { get; }
            public MimeMessage? Message { get; }
            public Exception? Error { get; }
        }
    }
}
