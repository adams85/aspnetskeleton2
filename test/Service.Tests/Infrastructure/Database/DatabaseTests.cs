using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Core;
using WebApp.DataAccess.Entities;
using WebApp.Service.Infrastructure.Database;
using WebApp.Service.Tests.TestData;
using Xunit;

namespace WebApp.Service.Tests.Infrastructure.Database
{
    public class DatabaseTests
    {
        [Fact]
        public async Task TestCaseGotDedicatedDbInstance1()
        {
            var testContextBuilder = new TestContextBuilder();
            testContextBuilder.AddDatabase();

            await using (var testContext = await testContextBuilder.BuildAsync())
            {
                await using (var dbContext = testContext.CreateWritableDbContext())
                {
                    dbContext.Roles.Add(new Role { RoleName = "X" });
                    await dbContext.SaveChangesAsync();
                }

                await using (var dbContext = testContext.CreateReadOnlyDbContext())
                {
                    Assert.Equal(1, await dbContext.Roles.CountAsync(role => role.RoleName == "X"));
                    Assert.Equal(1, await dbContext.Roles.CountAsync());
                }
            }
        }

        [Fact]
        public async Task TestCaseGotDedicatedDbInstance2()
        {
            var testContextBuilder = new TestContextBuilder();
            testContextBuilder.AddDatabase();

            await using (var testContext = await testContextBuilder.BuildAsync())
            {
                await using (var dbContext = testContext.CreateWritableDbContext())
                {
                    dbContext.Roles.Add(new Role { RoleName = "Y" });
                    await dbContext.SaveChangesAsync();
                }

                await using (var dbContext = testContext.CreateReadOnlyDbContext())
                {
                    Assert.Equal(1, await dbContext.Roles.CountAsync(role => role.RoleName == "Y"));
                    Assert.Equal(1, await dbContext.Roles.CountAsync());
                }
            }
        }

        [Fact]
        public async Task SeedDefaults()
        {
            var testContextBuilder = new TestContextBuilder();
            testContextBuilder.AddDatabase()
                .SeedDefaults();

            await using (var testContext = await testContextBuilder.BuildAsync())
            {
                await using (var dbContext = testContext.CreateReadOnlyDbContext())
                {
                    Assert.True(await dbContext.Roles.CountAsync() > 0);

                    var admin = await dbContext.Users
                        .Include(user => user.Profile)
                        .SingleOrDefaultAsync(user => user.UserName == ApplicationConstants.BuiltInRootUserName);

                    Assert.NotNull(admin);
                    Assert.NotNull(admin.Profile);
                }
            }
        }

        [Fact]
        public async Task SeedDefaultsAndCsvData()
        {
            var testContextBuilder = new TestContextBuilder();

            testContextBuilder.AddDatabase()
                .SeedDefaults()
                .SeedDataset(Datasets.Dataset1);

            await using (var testContext = await testContextBuilder.BuildAsync())
            {
                await using (var dbContext = testContext.CreateReadOnlyDbContext())
                {
                    Assert.True(await dbContext.Roles.CountAsync() > 0);

                    // ...
                }
            }
        }
    }
}
