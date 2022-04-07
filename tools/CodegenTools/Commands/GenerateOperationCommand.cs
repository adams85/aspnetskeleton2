using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace CodegenTools.Commands;

public abstract class GenerateOperationCommand : ICommand
{
    protected const string ServiceProjectName = "Service";
    protected const string ServiceContractProjectName = "Service.Contract";

    protected static void WriteAllTextSafe(string filePath, string content)
    {
        if (File.Exists(filePath))
            File.Move(filePath, filePath + ".bak", overwrite: true);

        File.WriteAllText(filePath, content);
    }

    protected GenerateOperationCommand() { }

    [Argument(0, "group", Description = "Group name")]
    [Required]
    public string Group { get; set; } = null!;

    [Required]
    public abstract string Name { get; set; }

    [Option("--root-path <PATH>", Description = "The location of the root directory of the solution. If not specified, it will be auto-detected by looking for sln files in the current directory and upwards.")]
    public string? RootPath { get; set; }

    [Option("--namespace <NAMESPACE>", Description = "The base namespace for generated classes. Default: 'WebApp'.")]
    public string? Namespace { get; set; }

    protected string GetProjectPath(string rootPath, string projectName) => Path.Combine(rootPath, "src", projectName);

    private bool TryLocateRootPath(out string? value)
    {
        string? currentPath = Environment.CurrentDirectory;
        do
        {
            if (Directory.EnumerateFiles(currentPath, "*.sln", SearchOption.TopDirectoryOnly).Any() &&
                Directory.Exists(GetProjectPath(currentPath, ServiceContractProjectName)))
            {
                value = currentPath;
                return true;
            }
        }
        while ((currentPath = Path.GetDirectoryName(currentPath)) != null);

        value = default;
        return false;
    }

    protected string GetRootPath()
    {
        var rootPath = RootPath;
        if (rootPath != null)
        {
            var serviceContractProjectPath = GetProjectPath(rootPath, ServiceContractProjectName);

            if (!Directory.Exists(rootPath))
                throw new CommandException("The specified root directory does not exist.");

            if (!Directory.Exists(serviceContractProjectPath))
                throw new CommandException($"The specified root directory is invalid. Project directory '{serviceContractProjectPath}' does not exist.");
        }
        else if (!TryLocateRootPath(out rootPath!))
        {
            throw new CommandException("Root directory was not found.");
        }

        return rootPath;
    }

    protected string GetNamespace() => Namespace ?? "WebApp";

    public abstract Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken);
}
