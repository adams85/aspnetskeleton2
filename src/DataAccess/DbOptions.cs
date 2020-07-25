namespace WebApp.DataAccess
{
    public class DbOptions
    {
        public static readonly string DefaultSectionName = "Database";

        public string Provider { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
        public string? ServerVersion { get; set; }
        public string? CharacterEncoding { get; set; }
        public string? CaseSensitiveCollation { get; set; }
        public string? CaseInsensitiveCollation { get; set; }
    }
}
