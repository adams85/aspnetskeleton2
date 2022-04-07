using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Environment;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace POTools.Commands;

[Command("scan", Description = "Scans for source files in a C# project or a directory and writes the file paths to the standard output.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect, AllowArgumentSeparator = true)]
internal class ScanCommand : ICommand
{
    private static readonly HashSet<string> s_projectExtensionFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".csproj" };

    private static readonly HashSet<string> s_compileExtensionFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cs" };
    private static readonly HashSet<string> s_contentExtensionFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cshtml" };
    private static readonly HashSet<string> s_extensionFilter = s_compileExtensionFilter.Concat(s_contentExtensionFilter).ToHashSet(StringComparer.OrdinalIgnoreCase);

    private readonly CommandLineContext _context;

    public ScanCommand(CommandLineContext context)
    {
        _context = context;
    }

    [Option("-p|--project-path", Description = "The path to an MSBuild file or a directory in which to look for source files. If omitted, the project file in the current directory is used, or the current directory if no or multiple project files exist.")]
    public string? ProjectPath { get; set; }

    [Option("-f|--framework <FRAMEWORK>", Description = "The target framework to use (when scanning an MSBuild file).")]
    public string? TargetFramework { get; set; }

    [Option("-c|--configuration <CONFIGURATION>", Description = "The configuration to use (when scanning an MSBuild file).")]
    public string? Configuration { get; set; }

    [Option("--include-links", Description = "Also include source files added as links.")]
    public bool IncludeLinks { get; set; }

    public string[] RemainingArguments { get; private set; } = null!;

    public Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken)
    {
        bool isMSBuildFile;

        var path = ProjectPath;
        if (path == null)
        {
            path = Directory.GetCurrentDirectory();

            var projectFiles = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly)
                .Where(p => s_projectExtensionFilter.Contains(Path.GetExtension(p)))
                .Take(2)
                .ToArray();

            if (projectFiles.Length == 1)
            {
                path = projectFiles[0];
                isMSBuildFile = true;
            }
            else
                isMSBuildFile = false;
        }
        else
        {
            path = Path.GetFullPath(path);
            isMSBuildFile = !Directory.Exists(path);
        }

        IEnumerable<string> filePaths;
        if (isMSBuildFile)
        {
            var originalWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.GetDirectoryName(path)!;
            try
            {
                // https://daveaglick.com/posts/running-a-design-time-build-with-msbuild-apis
                var analyzerManager = new AnalyzerManager();
                var project = analyzerManager.GetProject(path);

                var environmentOptions = new EnvironmentOptions();

                if (Configuration != null)
                    environmentOptions.GlobalProperties["Configuration"] = Configuration;

                Array.ForEach(RemainingArguments, arg => environmentOptions.Arguments.Add(arg));

                IAnalyzerResult? analyzerResult;
                if (TargetFramework != null)
                {
                    var buildEnvironment = project.EnvironmentFactory.GetBuildEnvironment(TargetFramework, environmentOptions);
                    var analyzerResults = project.Build(buildEnvironment);
                    analyzerResult = analyzerResults?.FirstOrDefault(result => result.TargetFramework == TargetFramework);
                    if (analyzerResult == null)
                        throw new CommandException($"Unable to load MSBuild file \"{path}\" using target framework '{TargetFramework}'.");
                }
                else
                {
                    var buildEnvironment = project.EnvironmentFactory.GetBuildEnvironment(environmentOptions);
                    var analyzerResults = project.Build(buildEnvironment);
                    analyzerResult = analyzerResults?.FirstOrDefault();
                    if (analyzerResult == null)
                        throw new CommandException($"Unable to load MSBuild file \"{path}\".");
                }

                var basePath = Path.GetDirectoryName(path)!;

                if (!analyzerResult.Items.TryGetValue("Compile", out var compileItems))
                    compileItems = Array.Empty<ProjectItem>();

                if (!analyzerResult.Items.TryGetValue("Content", out var contentItems))
                    contentItems = Array.Empty<ProjectItem>();

                var projectItems = compileItems.Where(projectItem => s_compileExtensionFilter.Contains(Path.GetExtension(projectItem.ItemSpec)))
                    .Concat(contentItems.Where(projectItem => s_contentExtensionFilter.Contains(Path.GetExtension(projectItem.ItemSpec))));

                if (!IncludeLinks)
                    projectItems = projectItems.Where(projectItem => !projectItem.Metadata.Keys.Contains("Link"));

                filePaths = projectItems
                    .Select(projectItem => Path.GetFullPath(Path.Combine(basePath, projectItem.ItemSpec)));
            }
            finally
            {
                Environment.CurrentDirectory = originalWorkingDirectory;
            }
        }
        else
            filePaths = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                .Where(p => s_extensionFilter.Contains(Path.GetExtension(p)));

        foreach (var filePath in filePaths)
            _context.Console.WriteLine(filePath);

        return Task.FromResult(0);
    }
}
