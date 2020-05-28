using Microsoft.Extensions.DependencyInjection;

namespace WebApp.DataAccess
{
    public class DataAccessOptions
    {
        public DbOptions Database { get; set; } = null!;
        public bool EnableSqlLogging { get; set; }
        public ServiceLifetime? DbContextLifetime { get; set; }
    }
}
