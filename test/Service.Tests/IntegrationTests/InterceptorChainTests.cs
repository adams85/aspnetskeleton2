using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Service.Roles;
using WebApp.Service.Tests.Infrastructure;
using WebApp.Service.Users;
using Xunit;

namespace WebApp.Service.Infrastructure
{
    public class InterceptorChainTests
    {
        [Fact]
        public async Task CommandDataAnnotationsValidatorInterceptorTest()
        {
            var testContextBuilder = TestContextBuilder.CreateDefault(builder => builder
                .AddServices(services => services.AddServiceLayer(TestContextBuilder.DefaultOptionsProvider))
                .AddDatabase(addDataAccessServices: false));

            using var testContext = await testContextBuilder.BuildAsync();

            var command = new AddUsersToRolesCommand { UserNames = new string[] { }, RoleNames = new string[] { "x" } };

            var commandDispatcher = testContext.Services.GetRequiredService<ICommandDispatcher>();

            var ex = await Assert.ThrowsAsync<ServiceErrorException>(
                async () => await commandDispatcher.DispatchAsync(command, default));

            Assert.Equal(ServiceErrorCode.ParamNotValid, ex.ErrorCode);
            Assert.Equal(new[] { nameof(command.UserNames) }, ex.Args);
        }

        [Fact]
        public async Task QueryDataAnnotationsValidatorInterceptorTest()
        {
            var testContextBuilder = TestContextBuilder.CreateDefault(builder => builder
                .AddServices(services => services.AddServiceLayer(TestContextBuilder.DefaultOptionsProvider))
                .AddDatabase(addDataAccessServices: false));

            using var testContext = await testContextBuilder.BuildAsync();

            var query = new GetUserQuery { };

            var queryDispatcher = testContext.Services.GetRequiredService<IQueryDispatcher>();

            var ex = await Assert.ThrowsAsync<ServiceErrorException>(
                async () => await queryDispatcher.DispatchAsync(query, default));

            Assert.Equal(ServiceErrorCode.ParamNotSpecified, ex.ErrorCode);
            Assert.Equal(new[] { nameof(query.Identifier) }, ex.Args);
        }
    }
}
