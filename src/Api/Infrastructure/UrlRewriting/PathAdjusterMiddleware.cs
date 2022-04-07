using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace WebApp.Api.Infrastructure.UrlRewriting;

public sealed class PathAdjusterMiddleware
{
    private static bool AdjustPath(HttpRequest request, PathString originalPrefix, PathString newPrefix)
    {
        bool isMatch;
        if (originalPrefix.HasValue)
        {
            if (request.PathBase.HasValue)
            {
                isMatch = request.PathBase.StartsWithSegments(originalPrefix, out var remaining);
                if (isMatch)
                    request.PathBase = remaining;
            }
            else
            {
                isMatch = request.Path.StartsWithSegments(originalPrefix, out var remaining);
                if (isMatch)
                    request.Path = remaining;
            }
        }
        else
            isMatch = true;

        if (isMatch && newPrefix.HasValue)
            request.PathBase = newPrefix + request.PathBase;

        return isMatch;
    }

    private readonly RequestDelegate _next;

    private readonly Func<HttpRequest, bool>[] _pathAdjusters;

    public PathAdjusterMiddleware(RequestDelegate next, IOptions<PathAdjusterOptions>? options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        _pathAdjusters = (options?.Value?.PathAdjustments ?? Enumerable.Empty<PathAdjustment>())
            .Select(adj => new Func<HttpRequest, bool>(req => AdjustPath(req, adj.OriginalPrefix, adj.NewPrefix)))
            .ToArray();
    }

    public Task Invoke(HttpContext context)
    {
        for (int i = 0, n = _pathAdjusters.Length; i < n; i++)
            if (_pathAdjusters[i](context.Request))
                break;

        return _next(context);
    }
}
