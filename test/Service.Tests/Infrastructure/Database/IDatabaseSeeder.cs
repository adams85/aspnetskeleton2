using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess;

namespace WebApp.Service.Tests.Infrastructure.Database;

public interface IDatabaseSeeder
{
    Task SeedAsync(IDbContextFactory<WritableDataContext> dbContextFactory, CancellationToken cancellationToken = default);
}
