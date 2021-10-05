namespace WebApp.DataAccess
{
    public class DataAccessOptions
    {
        public DbOptions Database { get; set; } = null!;
        public bool EnableSqlLogging { get; set; }
        public int? DbContextPoolSize { get; set; }
    }
}
