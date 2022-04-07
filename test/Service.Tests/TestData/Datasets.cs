using System.IO;
using WebApp.DataAccess.Entities;
using WebApp.Service.Tests.Infrastructure;

namespace WebApp.Service.Tests.TestData;

public class Datasets
{
    public static readonly CsvFile[] Dataset1 =
    {
        new CsvFile { FilePath = Path.Combine(TestContext.BasePath, "TestData", "Dataset1", "001-Roles.csv"), EntityType = typeof(Role) },
        new CsvFile { FilePath = Path.Combine(TestContext.BasePath, "TestData", "Dataset1", "002-Users.csv"), EntityType = typeof(User) },
        new CsvFile { FilePath = Path.Combine(TestContext.BasePath, "TestData", "Dataset1", "003-Profiles.csv"), EntityType = typeof(Profile) },
        new CsvFile { FilePath = Path.Combine(TestContext.BasePath, "TestData", "Dataset1", "004-UserRoles.csv"), EntityType = typeof(UserRole) },
    };
}
