using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebApp.Service;

#if SERVICE_HOST
namespace WebApp.Service.Host.Infrastructure
#else
namespace WebApp.Api.Infrastructure
#endif
{
    public sealed class HttpExecutionContext : OperationExecutionContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpExecutionContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public override ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    }
}
