using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.PO;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using POTools.Helpers;
using POTools.Services.Extracting;

namespace POTools.Commands;

[Command("extract", Description = "Extracts localizable text from source files in PO format.")]
internal class ExtractCommand : ICommand
{
    private static readonly POGeneratorSettings s_generatorSettings = new POGeneratorSettings { IgnoreEncoding = true };

    private readonly CommandLineContext _context;

    private string? _basePath;
    private Dictionary<string, ExtractResult>? _results;

    public ExtractCommand(CommandLineContext context)
    {
        _context = context;
    }

    [Option("-p|--base-path", Description = "Base path of the source files. If omitted, the current directory.")]
    public string? BasePath { get; set; }

    [Option("-i|--input <PATH>", Description = "Path to a source file from which to extract text. If omitted, the list is read from the standard input. Can be specified multiple times.")]
    public string[]? SourceFilePaths { get; private set; }

    [Option("-o|--output <PATH>", Description = "Path to the output PO template (POT) file. If omitted, the file content is written to the standard output.")]
    public string? TemplateFilePath { get; set; }

    [Option("-l|--language <CODE>", Description = "Create a PO file with the specified language. Applies only when output file path is specified. Can be specified multiple times.")]
    public string[]? Languages { get; private set; }

    [Option("-m|--merge", Description = "Merge newly extracted entries into the previously created catalog if exists. Applies only when output file path is specified.")]
    public bool Merge { get; set; }

    [Option("--no-backup", Description = "Don't backup existing output files.")]
    public bool NoBackup { get; set; }

    [Option("--no-refs", Description = "Don't add source references.")]
    public bool NoReferences { get; set; }

    [Option("--no-comments", Description = "Don't add extracted comments.")]
    public bool NoComments { get; set; }

    public string[] RemainingArguments { get; private set; } = null!;

    private IList<string> GetSourceFilePaths()
    {
        if (SourceFilePaths != null && SourceFilePaths.Length > 0)
            return SourceFilePaths;

        var inputFilePaths = new List<string>();

        string? value;
        while (!string.IsNullOrEmpty(value = _context.Console.In.ReadLine()))
            inputFilePaths.Add(value.Trim());

        return inputFilePaths;
    }

    private string? GetTemplateFilePath() =>
        TemplateFilePath != null && Path.GetExtension(TemplateFilePath).Length == 0 ?
        Path.ChangeExtension(TemplateFilePath, ".pot") :
        TemplateFilePath;

    private ThreadData InitializeThread()
    {
        return new ThreadData();
    }

    private ThreadData Extract(string relativeFilePath, ParallelLoopState state, ThreadData data)
    {
        var extension = Path.GetExtension(relativeFilePath);
        var extractor = data.GetExtractor(extension);
        if (extractor == null)
            return data;

        string content;
        LocalizableTextInfo[] texts;
        try
        {
            content = File.ReadAllText(Path.Combine(_basePath!, relativeFilePath));
            texts = extractor.Extract(content).ToArray();
        }
        catch (Exception ex)
        {
            data.Results.Add(relativeFilePath, new ExtractResult { Error = ex.Message.Replace(Environment.NewLine, " ") });
            return data;
        }

        if (texts.Length > 0)
            data.Results.Add(relativeFilePath, new ExtractResult { Texts = texts });

        return data;
    }

    private void FinalizeThread(ThreadData data)
    {
        lock (_results!)
            foreach (var kvp in data.Results)
                _results.Add(kvp.Key, kvp.Value);
    }

    private static IEnumerable<string> GetPOEntryFlags(IPOEntry entry) => entry.Comments?
        .OfType<POFlagsComment>()
        .Where(flagsComment => flagsComment.Flags != null && flagsComment.Flags.Count > 0)
        .SelectMany(flagsComment => flagsComment.Flags) ?? Enumerable.Empty<string>();

    private POCatalog BuildTemplateCatalog(string? filePath, KeyValuePair<string, ExtractResult>[] fileTexts)
    {
        POCatalog? originalCatalog;
        if (File.Exists(filePath))
            using (var reader = new StreamReader(filePath!))
            {
                var parseResult = new POParser().Parse(reader);
                if (!parseResult.Success)
                {
                    var diagnosticMessages = parseResult.Diagnostics
                        .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

                    throw new CommandException($"Template file \"{filePath}\" is invalid: {string.Join(Environment.NewLine, diagnosticMessages)}");
                }

                originalCatalog = parseResult.Catalog;

                for (var i = originalCatalog.Count - 1; i >= 0; i--)
                {
                    var originalEntry = originalCatalog[i];

                    if (GetPOEntryFlags(originalEntry).Contains("removed"))
                        originalCatalog.RemoveAt(i);
                }
            }
        else
            originalCatalog = null;

        var groupsById = fileTexts
            .Where(fileText => fileText.Value.Texts != null)
            .SelectMany(fileText => fileText.Value.Texts!.Select(text => (text, fileText.Key)))
            .GroupBy(item => new POKey(item.text.Id, item.text.PluralId, item.text.ContextId))
            .OrderBy(item => item.Key, POKeyComparer.Instance);

        var catalog = new POCatalog();

        foreach (var groupById in groupsById)
        {
            var key = groupById.Key;

            foreach (var (text, sourceFilePath) in groupById)
            {
                if (!catalog.TryGetValue(key, out var entry))
                {
                    var state = "new";

                    if (originalCatalog != null && originalCatalog.TryGetValue(key, out var originalEntry))
                    {
                        var hasChanged =
                            key.Id != originalEntry[0] ||
                            key.PluralId != null && (originalEntry.Count != 2 || key.PluralId != originalEntry[1]);

                        state = hasChanged ? "changed" : null;
                    }

                    entry =
                        key.PluralId == null ?
                        (IPOEntry)new POSingularEntry(key) { Translation = key.Id } :
                        new POPluralEntry(key) { key.Id, key.PluralId };

                    entry.Comments = new List<POComment>();

                    if (state != null)
                        entry.Comments.Add(new POFlagsComment { Flags = new HashSet<string> { state } });

                    if (!NoReferences)
                        entry.Comments.Add(new POReferenceComment() { References = new List<POSourceReference>() });

                    catalog.Add(entry);
                }

                if (!NoReferences)
                {
                    var referenceComment = entry.Comments.OfType<POReferenceComment>().First();
                    referenceComment.References.Add(new POSourceReference(sourceFilePath, text.LineNumber));
                }

                if (!NoComments && !string.IsNullOrEmpty(text.Comment))
                    entry.Comments.Add(new POExtractedComment { Text = text.Comment });
            }
        }

        if (originalCatalog != null)
            foreach (var originalEntry in originalCatalog)
                if (!catalog.Contains(originalEntry.Key))
                {
                    const string entryRemovedMessage = "***THIS ENTRY WAS REMOVED. DO NOT TRANSLATE!***";

                    var entry =
                        originalEntry.Key.PluralId == null ?
                        (IPOEntry)new POSingularEntry(originalEntry.Key) { Translation = entryRemovedMessage } :
                        new POPluralEntry(originalEntry.Key) { entryRemovedMessage };

                    entry.Comments = new List<POComment> { new POFlagsComment { Flags = new HashSet<string> { "removed" } } };
                    catalog.Add(entry);
                }

        return catalog;
    }

    private static POCatalog BuildCatalog(string filePath, POCatalog templateCatalog)
    {
        POCatalog? originalCatalog;
        if (File.Exists(filePath))
            using (var reader = new StreamReader(filePath))
            {
                var parseResult = new POParser().Parse(reader);
                if (!parseResult.Success)
                {
                    var diagnosticMessages = parseResult.Diagnostics
                        .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

                    throw new CommandException($"Template file \"{filePath}\" is invalid: {string.Join(Environment.NewLine, diagnosticMessages)}");
                }

                originalCatalog = parseResult.Catalog;
            }
        else
            originalCatalog = null;

        var catalog = new POCatalog();

        foreach (var templateEntry in templateCatalog)
        {
            var flags = new HashSet<string>(GetPOEntryFlags(templateEntry));
            if (flags.Contains("removed"))
                continue;

            IEnumerable<string> originalFlags;
            if (originalCatalog != null && originalCatalog.TryGetValue(templateEntry.Key, out var originalEntry))
                originalFlags = GetPOEntryFlags(originalEntry);
            else
                (originalFlags, originalEntry) = (Enumerable.Empty<string>(), null);

            var isNew = flags.Remove("new");
            var hasChanged = flags.Remove("changed");
            var isOriginalFuzzy = originalFlags.Contains("fuzzy");

            IPOEntry entry = (hasChanged ? templateEntry : (originalEntry ?? templateEntry)) switch
            {
                POSingularEntry singularEntry => new POSingularEntry(templateEntry.Key) { Translation = singularEntry.Translation },
                POPluralEntry pluralEntry => new POPluralEntry(templateEntry.Key, pluralEntry),
                _ => throw new InvalidOperationException()
            };

            if (isNew || hasChanged || isOriginalFuzzy)
            {
                flags.Add("fuzzy");
                entry.Comments = templateEntry.Comments?.Where(comment => !(comment is POFlagsComment)).ToList() ?? new List<POComment>();
                entry.Comments.Add(new POFlagsComment { Flags = flags });
            }
            else
                entry.Comments = templateEntry.Comments;

            catalog.Add(entry);
        }

        return catalog;
    }

    private void WriteCatalog(TextWriter writer, POCatalog catalog, CultureInfo? culture)
    {
        var now = DateTimeOffset.Now;

        try { catalog.Encoding = Encoding.GetEncoding(writer.Encoding.CodePage).BodyName; }
        catch (NotSupportedException) { catalog.Encoding = "(n/a)"; }

        if (culture != null)
        {
            catalog.Language = culture.Name.Replace('-', '_');

            if (PluralFormHelper.TryGetPluralForm(culture, out var pluralFormCount, out var pluralFormSelector))
            {
                (catalog.PluralFormCount, catalog.PluralFormSelector) = (pluralFormCount, pluralFormSelector);

                for (int i = 0, n = catalog.Count; i < n; i++)
                    EnsureTranslationCount(catalog[i], pluralFormCount);
            }

            static void EnsureTranslationCount(IPOEntry entry, int pluralFormCount)
            {
                if (entry is POPluralEntry pluralEntry && pluralEntry.Count > pluralFormCount)
                    for (int i = pluralEntry.Count - 1; i >= pluralFormCount; i--)
                        pluralEntry.RemoveAt(i);
            }
        }

        catalog.Headers = new Dictionary<string, string>
        {
            [POCatalog.ProjectIdVersionHeaderName] = string.Empty,
            [POCatalog.ReportMsgidBugsToHeaderName] = string.Empty,
            [POCatalog.PotCreationDateHeaderName] = $"{now:yyyy-MM-dd hh:mm}{(now.Offset >= TimeSpan.Zero ? "+" : "-")}{now.Offset:hhmm}",
            [POCatalog.PORevisionDateHeaderName] = string.Empty,
            [POCatalog.LastTranslatorHeaderName] = string.Empty,
            [POCatalog.LanguageTeamHeaderName] = string.Empty,
        };

        if (culture != null)
            catalog.HeaderComments = new[]
            {
                new POFlagsComment() { Flags = new HashSet<string> { "fuzzy" } }
            };

        var generator = new POGenerator(s_generatorSettings);
        generator.Generate(writer, catalog);

        writer.Flush();
    }

    private void SaveCatalog(string filePath, POCatalog catalog, CultureInfo? culture)
    {
        if (!NoBackup && File.Exists(filePath))
            File.Copy(filePath, filePath + ".bak", overwrite: true);

        using (var writer = new StreamWriter(filePath, append: false))
            WriteCatalog(writer, catalog, culture);
    }

    public Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken)
    {
        _basePath = BasePath != null ? Path.GetFullPath(BasePath) : Directory.GetCurrentDirectory();

        var relativeSourceFilePaths = GetSourceFilePaths().Select(filePath => Path.GetRelativePath(_basePath, filePath));
        var templateFilePath = GetTemplateFilePath();

        var cultures = Languages?.Select(language => CultureInfo.GetCultureInfo(language)).ToArray() ?? Array.Empty<CultureInfo>();

        // extracting texts
        _results = new Dictionary<string, ExtractResult>();
        Parallel.ForEach(relativeSourceFilePaths, InitializeThread, Extract, FinalizeThread);

        // generating output files
        var lookup = _results.ToLookup(kvp => kvp.Value.Success);

        var fileTexts = lookup[true].OrderBy(kvp => kvp.Key).ToArray();
        if (fileTexts.Length > 0)
        {
            var addReferences = !NoReferences;
            var addComments = !NoComments;

            var templateCatalog = BuildTemplateCatalog(Merge ? templateFilePath : null, fileTexts);

            if (templateFilePath != null)
            {
                SaveCatalog(templateFilePath, templateCatalog, null);

                if (cultures.Length > 0)
                {
                    var templateDirPath = Path.GetDirectoryName(templateFilePath)!;
                    var fileName = Path.GetFileNameWithoutExtension(templateFilePath) + ".po";

                    for (int i = 0, n = cultures.Length; i < n; i++)
                    {
                        var culture = cultures[i];
                        var dirPath = Path.Combine(templateDirPath, culture.Name);

                        var filePath = Path.Combine(dirPath, fileName);

                        var catalog = BuildCatalog(filePath, templateCatalog);

                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);

                        SaveCatalog(filePath, catalog, culture);
                    }
                }
            }
            else
                WriteCatalog(_context.Console.Out, templateCatalog, null);
        }

        // displaying errors
        var errors = lookup[false].OrderBy(kvp => kvp.Key).ToArray();
        if (errors.Length > 0)
        {
            _context.Console.Error.WriteLine("*** WARNING ***");
            _context.Console.Error.WriteLine("The following file(s) could not be processed:");

            var n = errors.Length;
            for (var i = 0; i < n; i++)
            {
                var error = errors[i];
                _context.Console.Error.WriteLine($"{error.Key} - {error.Value.Error}");
            }
        }
        return Task.FromResult(0);
    }

    private class ExtractResult
    {
        public LocalizableTextInfo[]? Texts { get; set; }
        public string? Error { get; set; }
        public bool Success => Error == null;
    }

    private class ThreadData
    {
        private readonly Dictionary<string, Lazy<ILocalizableTextExtractor>> _extractors = new Dictionary<string, Lazy<ILocalizableTextExtractor>>(StringComparer.OrdinalIgnoreCase)
        {
            { ".cs", new Lazy<ILocalizableTextExtractor>(() => new CSharpTextExtractor(), isThreadSafe: false) },
            { ".cshtml", new Lazy<ILocalizableTextExtractor>(() => new CSharpRazorTextExtractor(), isThreadSafe: false) },
            { ".resx", new Lazy<ILocalizableTextExtractor>(() => new ResourceTextExtractor(), isThreadSafe: false)  }
        };

        public ILocalizableTextExtractor? GetExtractor(string extension) =>
            _extractors.TryGetValue(extension, out var extractor) ? extractor.Value : null;

        public Dictionary<string, ExtractResult> Results = new Dictionary<string, ExtractResult>();
    }

    private class POKeyComparer : IComparer<POKey>
    {
        public static readonly POKeyComparer Instance = new POKeyComparer();

        private POKeyComparer() { }

        public int Compare(POKey x, POKey y)
        {
            var indicator = string.CompareOrdinal(x.Id, y.Id);
            if (indicator != 0)
                return indicator;

            indicator = string.CompareOrdinal(x.PluralId, y.PluralId);
            if (indicator != 0)
                return indicator;

            return string.CompareOrdinal(x.ContextId, y.ContextId);
        }
    }
}
