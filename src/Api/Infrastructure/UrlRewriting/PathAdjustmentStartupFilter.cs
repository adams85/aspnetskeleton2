using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebApp.Api.Infrastructure.UrlRewriting;

// https://andrewlock.net/exploring-istartupfilter-in-asp-net-core/
public sealed class PathAdjustmentStartupFilter : IStartupFilter
{
    public static IApplicationBuilder Configure(IApplicationBuilder app)
    {
        var pathAdjusterOptions = app.ApplicationServices.GetRequiredService<IOptions<PathAdjusterOptions>>();

        if (pathAdjusterOptions.Value.PathAdjustments.Count > 0)
            app.UseMiddleware<PathAdjusterMiddleware>(pathAdjusterOptions);

        return app;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app => next(Configure(app));
}
