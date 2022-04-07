using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Common.Roles;
using WebApp.Service.Roles;
using WebApp.Service.Tests.Infrastructure;
using WebApp.Service.Tests.TestData;
using Xunit;

namespace WebApp.Service.Tests.UnitTests.Users;

public class AddUsersToRolesCommandTests
{
    [Fact]
    public async Task AddSingleUserToSingleRole()
    {
        var testContextBuilder = TestContextBuilder.CreateDefault(builder => builder
            .AddDatabase()
                .SeedDefaults()
                .SeedDataset(Datasets.Dataset1));

        await using var testContext = await testContextBuilder.BuildAsync();

        var command = new AddUsersToRolesCommand
        {
            UserNames = new[] { "JohnDoe" },
            RoleNames = new[] { nameof(RoleEnum.Administrators) },
        };

        var commandContext = new CommandContext(command, testContext.Services);

        var handler = ActivatorUtilities.CreateInstance<AddUsersToRolesCommandHandler>(testContext.Services);

        await handler.HandleAsync(command, commandContext, default);

        await using (var dbContext = testContext.CreateReadOnlyDbContext())
        {
            var user = await dbContext.Users
                .Include(user => user.Roles!).ThenInclude(userRole => userRole.Role)
                .FirstAsync(user => user.UserName == command.UserNames[0]);

            Assert.Equal(1, user.Roles?.Count);
            Assert.Contains(user.Roles, userRole => userRole.Role.RoleName == command.RoleNames[0]);
        }
    }
}
