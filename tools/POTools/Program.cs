using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using POTools.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace POTools
{
    [Command("po", FullName = "CLI tools for extracting localizable text from source files in PO format")]
    [HelpOption(Inherited = true)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(ScanCommand), typeof(ExtractCommand))]
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var console = PhysicalConsole.Singleton;

            using var app = new CommandLineApplication<Program>(console, Environment.CurrentDirectory);
            app.Conventions.UseDefaultConventions();
            try
            {
                return await app.ExecuteAsync(args);
            }
            catch (OperationCanceledException)
            {
                console.WriteLine("Command was canceled.");
                return 0;
            }
            catch (Exception ex) when (ex is CommandException || ex is CommandParsingException)
            {
                console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private readonly IConsole _console;

        public Program(IConsole console)
        {
            _console = console;
        }

        private Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken)
        {
            _console.Error.WriteLine("A command must be specified.");
            _console.Error.WriteLine();

            app.ShowHelp();
            return Task.FromResult(1);
        }

        private static string GetVersion()
        {
            var assembly = typeof(Program).Assembly;
            return
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
                assembly.GetName().Version!.ToString();
        }
    }
}
