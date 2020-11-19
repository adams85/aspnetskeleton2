namespace CodegenTools.Templates
{
    public abstract class QueryTemplateBase : TemplateBase
    {
        public string Namespace { get; set; } = null!;
        public string Group { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? ResultType { get; set; }
        public string? ListItemType { get; set; }
        public bool UseGenericListResult { get; set; }
        public bool IsProgressReporter { get; set; }

        public bool IsList => !string.IsNullOrEmpty(ListItemType);

        public string GetResultType()
        {
            if (!string.IsNullOrEmpty(ResultType))
                return ResultType;

            return
                !IsList || !UseGenericListResult ?
                $"{Name}Result" :
                $"ListResult<{ListItemType}>";
        }

        public string GetQueryBaseType(string resultType)
        {
            return !IsList ? $"IQuery<{resultType}>" : $"ListQuery<{resultType}>";
        }

        public string GetQueryHandlerBaseType(string resultType)
        {
            return
                !IsList ? $"QueryHandler<{Name}Query, {resultType}>" :
                UseGenericListResult ? $"ListQueryHandler<{Name}Query, {ListItemType}>" :
                $"ListQueryHandler<{Name}Query, {resultType}, {ListItemType}>";
        }
    }
}
