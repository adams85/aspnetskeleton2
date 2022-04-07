namespace WebApp.Service.Infrastructure;

internal sealed class DefaultExecutionContextAccessor : IExecutionContextAccessor
{
    public OperationExecutionContext ExecutionContext { get; } = OperationExecutionContext.Default;
}
