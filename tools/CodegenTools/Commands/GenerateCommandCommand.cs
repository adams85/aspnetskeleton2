using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CodegenTools.Templates;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace CodegenTools.Commands
{
    [Command("cmd", Description = "Creates a command in the service layer by generating the necessary data and handler classes.")]
    internal class GenerateCommandCommand : GenerateOperationCommand
    {
        private readonly CommandLineContext _context;

        public GenerateCommandCommand(CommandLineContext context)
        {
            _context = context;
        }

        [Option("-k|--key-generator", Description = "If specified, infrastructure for notification of auto-generated IDs will be added to the command.")]
        public bool IsKeyGenerator { get; set; }

        [Option("-p|--progress-reporter", Description = "If specified, infrastructure for progress notification will be added to the command.")]
        public bool IsProgressReporter { get; set; }

        public override Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken)
        {
            void Generate<T>(string templateBasePath, string templateFileName) where T : CommandTemplateBase, new()
            {
                var templateFilePath = Path.Combine(templateBasePath, templateFileName);
                _context.Console.WriteLine($"Generating {templateFilePath}...");

                var content = new T
                {
                    Namespace = GetNamespace(),
                    Group = Group,
                    Name = Name,
                    IsKeyGenerator = IsKeyGenerator,
                    IsProgressReporter = IsProgressReporter,
                }.TransformText();

                Directory.CreateDirectory(templateBasePath);
                WriteAllTextSafe(templateFilePath, content);
            }

            var rootPath = GetRootPath();
            var modelsPath = Path.Combine(GetProjectPath(rootPath, ServiceContractProjectName), Group);
            var handlerPath = Path.Combine(GetProjectPath(rootPath, ServiceProjectName), Group);

            Generate<Command>(modelsPath, Name + "Command.cs");

            Generate<CommandHandler>(handlerPath, Name + "CommandHandler.cs");

            return Task.FromResult(0);
        }
    }
}
