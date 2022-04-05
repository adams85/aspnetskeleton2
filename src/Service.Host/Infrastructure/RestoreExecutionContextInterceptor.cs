using System;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

using static WebApp.Service.Host.ServiceHostConstants;

namespace WebApp.Service.Infrastructure
{
    internal sealed class RestoreExecutionContextInterceptor : Interceptor
    {
        private void LoadExecutionContextFrom(ServerCallContext context)
        {
            var headers = context.RequestHeaders;
            if (headers == null)
                return;

            Metadata.Entry? identityAuthenticationTypeEntry = null, identityNameEntry = null, cultureNameEntry = null, uiCultureNameEntry = null;
            for (int i = 0, n = headers.Count; i < n; i++)
            {
                var entry = headers[i];

                if (Match(entry, IdentityAuthenticationTypeHeaderName, isBinary: false, ref identityAuthenticationTypeEntry) ||
                    Match(entry, IdentityNameHeaderName, isBinary: true, ref identityNameEntry) ||
                    Match(entry, CultureNameHeaderName, isBinary: false, ref cultureNameEntry) ||
                    Match(entry, UICultureNameHeaderName, isBinary: false, ref uiCultureNameEntry))
                    continue;

                static bool Match(Metadata.Entry currentEntry, string headerName, bool isBinary, ref Metadata.Entry? entry)
                {
                    if (!headerName.Equals(currentEntry.Key, StringComparison.OrdinalIgnoreCase) || currentEntry.IsBinary != isBinary)
                        return false;

                    entry ??= currentEntry;
                    return true;
                }
            }

            if (cultureNameEntry == null || uiCultureNameEntry == null)
                throw new InvalidOperationException($"{CultureNameHeaderName} and {UICultureNameHeaderName} headers must always be specified.");

            var httpContext = context.GetHttpContext();
            ClaimsIdentity identity;

            if (identityAuthenticationTypeEntry != null && identityNameEntry != null)
            {
                var nameClaim = new Claim(ClaimTypes.Name, Encoding.UTF8.GetString(identityNameEntry.ValueBytes));
                identity = new ClaimsIdentity(new[] { nameClaim }, identityAuthenticationTypeEntry.Value);
            }
            else
                identity = new ClaimsIdentity();

            httpContext.User = new ClaimsPrincipal(identity);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureNameEntry.Value);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(uiCultureNameEntry.Value);
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LoadExecutionContextFrom(context);
            return base.ClientStreamingServerHandler(requestStream, context, continuation);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LoadExecutionContextFrom(context);
            return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LoadExecutionContextFrom(context);
            return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            LoadExecutionContextFrom(context);
            return base.UnaryServerHandler(request, context, continuation);
        }
    }
}
