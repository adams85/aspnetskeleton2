using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Core;
using WebApp.Service.Tests.Infrastructure;
using WebApp.Service.Tests.TestData;
using Xunit;

namespace WebApp.Service.Users;

public class ListUsersQueryTests
{
    [Fact]
    public async Task NoParams()
    {
        var testContextBuilder = TestContextBuilder.CreateDefault(builder => builder
            .AddDatabase()
                .SeedDefaults()
                .SeedDataset(Datasets.Dataset1));

        await using var testContext = await testContextBuilder.BuildAsync();

        var query = new ListUsersQuery { };

        var queryContext = new QueryContext(query, testContext.Services);

        var handler = ActivatorUtilities.CreateInstance<ListUsersQueryHandler>(testContext.Services);

        var result = await handler.HandleAsync(query, queryContext, default);

        Assert.Equal(3, result.Items?.Length);

        Assert.Contains(result.Items!, item => item.UserId == 1 && item.UserName == ApplicationConstants.BuiltInRootUserName);
        Assert.Contains(result.Items!, item => item.UserId == 11 && item.UserName == "JohnDoe");
        Assert.Contains(result.Items!, item => item.UserId == 12 && item.UserName == "JaneDoe");
    }

    [Fact]
    public async Task FilterByNamePagedAndSorted()
    {
        var testContextBuilder = TestContextBuilder.CreateDefault(builder => builder
            .AddDatabase()
                .SeedDefaults()
                .SeedDataset(Datasets.Dataset1));

        await using var testContext = await testContextBuilder.BuildAsync();

        var query = new ListUsersQuery
        {
            // TODO: SQLite doesn't respect case insensitivity when matching parts of strings
            UserNamePattern = "Doe",
            PageSize = 1,
            OrderBy = new[] { "+UserName" }
        };

        var queryContext = new QueryContext(query, testContext.Services);

        var handler = ActivatorUtilities.CreateInstance<ListUsersQueryHandler>(testContext.Services);

        var result = await handler.HandleAsync(query, queryContext, default);

        Assert.Equal(1, result.Items?.Length);
        Assert.Equal(2, result.TotalItemCount);

        Assert.Contains(result.Items!, item => item.UserId == 12 && item.UserName == "JaneDoe");
    }
}
