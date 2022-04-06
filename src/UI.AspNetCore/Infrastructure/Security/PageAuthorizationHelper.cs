using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace WebApp.UI.Infrastructure.Security
{
    public sealed class PageAuthorizationHelper : IPageAuthorizationHelper, IDisposable
    {
        private readonly EndpointDataSource _endpointDataSource;
        private readonly PageLoader _pageLoader;
        private readonly IAuthorizationPolicyProvider _policyProvider;

        private readonly ConcurrentDictionary<(string, string?), Endpoint> _endpointCache;
        private IDisposable? _endpointDataSourceChangeDisposable;

        public PageAuthorizationHelper(EndpointDataSource endpointDataSource, PageLoader pageLoader, IAuthorizationPolicyProvider policyProvider)
        {
            _endpointDataSource = endpointDataSource ?? throw new ArgumentNullException(nameof(endpointDataSource));
            _pageLoader = pageLoader ?? throw new ArgumentNullException(nameof(pageLoader));
            _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));

            _endpointCache = new ConcurrentDictionary<(string, string?), Endpoint>();

            _endpointDataSourceChangeDisposable = ChangeToken.OnChange(endpointDataSource.GetChangeToken, HandleEndpointDataSourceChange);
        }

        public void Dispose()
        {
            _endpointDataSourceChangeDisposable?.Dispose();
            _endpointDataSourceChangeDisposable = null;
        }

        private void HandleEndpointDataSourceChange()
        {
            _endpointCache.Clear();
        }

        private static bool IsMatchingActionDescriptor(PageActionDescriptor actionDescriptor, string pageName, string? areaName)
        {
            if (!actionDescriptor.RouteValues.TryGetValue("page", out var pageRouteValue) || pageRouteValue != pageName)
                return false;

            if (actionDescriptor.RouteValues.TryGetValue("area", out var areaRouteValue))
                return string.IsNullOrEmpty(areaName) ? string.IsNullOrEmpty(areaRouteValue) : areaName == areaRouteValue;
            else
                return string.IsNullOrEmpty(areaName);
        }

        private Endpoint FindActionDescriptor(IReadOnlyList<Endpoint> endpoints, string pageName, string? areaName) => _endpointCache.GetOrAdd((pageName, areaName), (key, endpoints) =>
        {
            var (pageName, areaName) = key;

            for (int i = 0, n = endpoints.Count; i < n; i++)
            {
                var endpoint = endpoints[i];

                var actionDescriptor = endpoint.Metadata.GetMetadata<PageActionDescriptor>();
                if (actionDescriptor != null &&
                    !endpoint.Metadata.OfType<IDynamicEndpointMetadata>().Any() &&
                    IsMatchingActionDescriptor(actionDescriptor, pageName, areaName))
                {
                    return endpoint;
                }
            }

            throw new ArgumentException("No matching endpoint was found.", nameof(pageName));
        }, endpoints);

        public async Task<bool> CheckAccessAllowedAsync(HttpContext httpContext, string pageName, string? areaName)
        {
            var endpoint = FindActionDescriptor(_endpointDataSource.Endpoints, pageName, areaName);

            // we need to load the page to ensure that it's endpoint metadata is populated (see GlobalPageApplicationModelConvention)
            var actionDescriptor = endpoint.Metadata.GetMetadata<PageActionDescriptor>()!;
            var compiledActionDescriptor = await _pageLoader.LoadAsync(actionDescriptor, endpoint.Metadata);

            // based on: https://github.com/dotnet/aspnetcore/blob/v6.0.3/src/Security/Authorization/Policy/src/AuthorizationMiddleware.cs#L44

            var endpointMetadata = compiledActionDescriptor.Endpoint?.Metadata ?? endpoint.Metadata;

            // Allow Anonymous skips all authorization
            if (endpointMetadata.GetMetadata<IAllowAnonymous>() != null)
                return true;

            var authorizeData = endpointMetadata.GetOrderedMetadata<IAuthorizeData>() ?? Array.Empty<IAuthorizeData>();
            var policy = await AuthorizationPolicy.CombineAsync(_policyProvider, authorizeData);
            if (policy == null)
                return true;

            // Policy evaluator has transient lifetime so it fetched from request services instead of injecting in constructor
            var policyEvaluator = httpContext.RequestServices.GetRequiredService<IPolicyEvaluator>();

            var authenticateResult =
                (httpContext.User?.Identity?.IsAuthenticated ?? false) ?
                AuthenticateResult.Success(new AuthenticationTicket(httpContext.User, "context.User")) :
                AuthenticateResult.NoResult();

            var authorizeResult = await policyEvaluator.AuthorizeAsync(policy, authenticateResult, httpContext, resource: endpoint);

            return authorizeResult.Succeeded;
        }
    }
}
