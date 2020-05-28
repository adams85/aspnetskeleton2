using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApp.Core;
using WebApp.Core.Infrastructure;
using WebApp.Service.Infrastructure;
using WebApp.Service.Proxy.Tests.IntegrationTests;
using WebApp.Service.Users;
using WebApp.Tests.Helpers;
using Xunit;

namespace WebApp.Service
{
    // TODO: add tests for progress reporting
    [Collection(nameof(ServiceHostCollection))]
    public class ProxyTests
    {
        private static IServiceProvider BuildProxyServices(string serviceBaseUrl)
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddDebug());
            services.Configure<ServiceProxyApplicationOptions>(options => options.ServiceBaseUrl = serviceBaseUrl);
            services.AddServiceLayer(new OptionsProvider());

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
                foreach (var initializer in scope.ServiceProvider.GetRequiredService<IEnumerable<IApplicationInitializer>>())
                    initializer.InitializeAsync(default).GetAwaiter().GetResult();

            return sp;
        }

        private static readonly IServiceProvider s_proxyServices = BuildProxyServices(ServiceHostFixture.ServiceBaseUrl);

        [Fact]
        public async Task DispatchCommandExpectingSuccess()
        {
            using var scope = s_proxyServices.CreateScope();

            // this operation should be a no-op
            var command = new RegisterUserActivityCommand
            {
                UserName = ApplicationConstants.BuiltInRootUserName,
                SuccessfulLogin = null,
                UIActivity = false,

            };

            var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

            await commandDispatcher.DispatchAsync(command, default);
        }

        [Fact]
        public async Task DispatchCommandExpectingFailure()
        {
            using var scope = s_proxyServices.CreateScope();

            // this operation should be a no-op
            var command = new RegisterUserActivityCommand
            {
                UserName = "",
                SuccessfulLogin = null,
                UIActivity = false,

            };

            var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

            var ex = await Assert.ThrowsAsync<ServiceErrorException>(async () => await commandDispatcher.DispatchAsync(command, default));

            Assert.Equal(ServiceErrorCode.ParamNotSpecified, ex.ErrorCode);
            Assert.Equal(new[] { nameof(command.UserName) }, ex.Args);
        }

        [Fact]
        public async Task DispatchQueryExpectingSuccess()
        {
            using var scope = s_proxyServices.CreateScope();

            var query = new ListUsersQuery
            {
                UserNamePattern = ApplicationConstants.BuiltInRootUserName,
            };

            var queryDispatcher = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();

            var result = await queryDispatcher.DispatchAsync(query, default);

            Assert.Equal(1, result.Items?.Length);
            Assert.Contains(result.Items, item => ApplicationConstants.BuiltInRootUserName.Equals(item.UserName, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task DispatchQueryExpectingFailure()
        {
            using var scope = s_proxyServices.CreateScope();

            var query = new GetUserQuery { };

            var queryDispatcher = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();

            var ex = await Assert.ThrowsAsync<ServiceErrorException>(async () => await queryDispatcher.DispatchAsync(query, default));

            Assert.Equal(ServiceErrorCode.ParamNotSpecified, ex.ErrorCode);
            Assert.Equal(new[] { nameof(query.Identifier) }, ex.Args);
        }
    }
}
