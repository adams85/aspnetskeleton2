namespace WebApp.Service.Infrastructure.Database
{
    public class DbInitializerOptions
    {
        public static readonly string DefaultSectionName = "Database";

        public bool EnsureCreated { get; set; } = true;
        public DbSeedObjects Seed { get; set; }
    }
}
