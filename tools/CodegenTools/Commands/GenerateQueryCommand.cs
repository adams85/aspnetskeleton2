using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CodegenTools.Templates;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace CodegenTools.Commands
{
    [Command("qry", Description = "Creates a query in the service layer by generating the necessary data and handler classes.")]
    internal class GenerateQueryCommand : GenerateOperationCommand
    {
        private readonly CommandLineContext _context;

        public GenerateQueryCommand(CommandLineContext context)
        {
            _context = context;
        }

        [Argument(1, "name", Description = "Query name")]
        public sealed override string Name { get; set; } = null!;

        [Option("-r|--result", Description = "If specified, no result type will be generated, but the query will use the specified result type.")]
        public string? ResultType { get; set; }

        [Option("-l|--list-item", Description = "If specified, list-enabled query of the specified item type will be generated.")]
        public string? ListItemType { get; set; }

        [Option("-g|--generic-list", Description = "Applies to list-enabled queries only. If present, no result type will be generated, but the query will use the generic one (ListResult<T>).")]
        public bool UseGenericListResult { get; set; }

        [Option("-e|--event-producer", Description = "If specified, infrastructure for notification of events will be added to the query.")]
        public bool IsEventProducer { get; set; }

        public override Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken)
        {
            void Generate<T>(string templateBasePath, string templateFileName) where T : QueryTemplateBase, new()
            {
                var templateFilePath = Path.Combine(templateBasePath, templateFileName);
                _context.Console.WriteLine($"Generating {templateFilePath}...");

                var content = new T
                {
                    Namespace = GetNamespace(),
                    Group = Group,
                    Name = Name,
                    ResultType = ResultType,
                    ListItemType = ListItemType,
                    UseGenericListResult = UseGenericListResult,
                    IsEventProducer = IsEventProducer,
                }.TransformText();

                Directory.CreateDirectory(templateBasePath);
                WriteAllTextSafe(templateFilePath, content);
            }

            var rootPath = GetRootPath();
            var modelsPath = Path.Combine(GetProjectPath(rootPath, ServiceContractProjectName), Group);
            var handlerPath = Path.Combine(GetProjectPath(rootPath, ServiceProjectName), Group);

            Generate<Query>(modelsPath, Name + "Query.cs");

            if (string.IsNullOrEmpty(ResultType) && (string.IsNullOrEmpty(ListItemType) || !UseGenericListResult))
                Generate<Result>(modelsPath, Name + "Result.cs");

            Generate<QueryHandler>(handlerPath, Name + "QueryHandler.cs");

            return Task.FromResult(0);
        }
    }
}
