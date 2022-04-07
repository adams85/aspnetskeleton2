namespace WebApp.Service.Infrastructure.Database;

public class DbInitializerOptions
{
    public const string DefaultSectionName = "Database";

    public bool EnsureCreated { get; set; } = true;
    public DbSeedObjects Seed { get; set; }
}
