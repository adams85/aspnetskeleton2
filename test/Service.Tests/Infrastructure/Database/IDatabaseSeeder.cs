using System.Threading;
using System.Threading.Tasks;
using WebApp.DataAccess;

namespace WebApp.Service.Tests.Infrastructure.Database
{
    public interface IDatabaseSeeder
    {
        Task SeedAsync(WritableDataContext dbContext, CancellationToken cancellationToken = default);
    }
}
