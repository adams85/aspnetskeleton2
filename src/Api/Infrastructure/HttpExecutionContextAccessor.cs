using Microsoft.AspNetCore.Http;
using WebApp.Service;
using WebApp.Service.Infrastructure;

#if SERVICE_HOST
namespace WebApp.Service.Host.Infrastructure;
#else
namespace WebApp.Api.Infrastructure;
#endif

public sealed class HttpExecutionContextAccessor : IExecutionContextAccessor
{
    public HttpExecutionContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        ExecutionContext = new HttpExecutionContext(httpContextAccessor);
    }

    public OperationExecutionContext ExecutionContext { get; }
}
