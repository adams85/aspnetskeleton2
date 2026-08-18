using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TUnit.Core;
using WebApp.Core;
using WebApp.Core.Helpers;
using WebApp.Service.Infrastructure;
using WebApp.Service.Proxy.Tests.IntegrationTests;
using WebApp.Service.Users;
using WebApp.Tests.Helpers;
using Xunit;

namespace WebApp.Service;

// TODO: add tests for progress reporting
public class ProxyTests
{
    private static IServiceProvider BuildProxyServices(string serviceBaseUrl)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());
        services.Configure<ServiceProxyApplicationOptions>(options => options.ServiceBaseUrl = serviceBaseUrl);
        services.AddServiceLayer(new OptionsProvider());

        var sp = services.BuildServiceProvider();

        sp.InitializeApplicationAsync(default).GetAwaiter().GetResult();

        return sp;
    }

    private static readonly IServiceProvider s_proxyServices = BuildProxyServices(ServiceHostFixture.ServiceBaseUrl);

    [Test]
    public async Task DispatchCommandExpectingSuccess()
    {
        await using var scope = AsyncDisposableAdapter.From(s_proxyServices.CreateScope());

        // this operation should be a no-op
        var command = new RegisterUserActivityCommand
        {
            UserName = ApplicationConstants.BuiltInRootUserName,
            SuccessfulLogin = null,
            UIActivity = false,

        };

        var commandDispatcher = scope.Value.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        await commandDispatcher.DispatchAsync(command, default);
    }

    [Test]
    public async Task DispatchCommandExpectingFailure()
    {
        await using var scope = AsyncDisposableAdapter.From(s_proxyServices.CreateScope());

        // this operation should be a no-op
        var command = new RegisterUserActivityCommand
        {
            UserName = "",
            SuccessfulLogin = null,
            UIActivity = false,

        };

        var commandDispatcher = scope.Value.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        var ex = await Assert.ThrowsAsync<ServiceErrorException>(async () => await commandDispatcher.DispatchAsync(command, default));

        Assert.Equal(ServiceErrorCode.ParamNotSpecified, ex.ErrorCode);
        Assert.Equal(new[] { nameof(command.UserName) }, ex.Args);
    }

    [Test]
    public async Task DispatchQueryExpectingSuccess()
    {
        await using var scope = AsyncDisposableAdapter.From(s_proxyServices.CreateScope());

        var query = new ListUsersQuery
        {
            UserNamePattern = ApplicationConstants.BuiltInRootUserName,
        };

        var queryDispatcher = scope.Value.ServiceProvider.GetRequiredService<IQueryDispatcher>();

        var result = await queryDispatcher.DispatchAsync(query, default);

        Assert.Equal(1, result.Items?.Length);
        Assert.Contains(result.Items!, item => ApplicationConstants.BuiltInRootUserName.Equals(item.UserName, StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public async Task DispatchQueryExpectingFailure()
    {
        await using var scope = AsyncDisposableAdapter.From(s_proxyServices.CreateScope());

        var query = new GetUserQuery { };

        var queryDispatcher = scope.Value.ServiceProvider.GetRequiredService<IQueryDispatcher>();

        var ex = await Assert.ThrowsAsync<ServiceErrorException>(async () => await queryDispatcher.DispatchAsync(query, default));

        Assert.Equal(ServiceErrorCode.ParamNotSpecified, ex.ErrorCode);
        Assert.Equal(new[] { nameof(query.Identifier) }, ex.Args);
    }
}
