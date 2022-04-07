namespace WebApp.Service.Infrastructure;

public interface IExecutionContextAccessor
{
    OperationExecutionContext ExecutionContext { get; }
}
