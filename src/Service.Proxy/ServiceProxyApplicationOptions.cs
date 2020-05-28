namespace WebApp.Service
{
    public class ServiceProxyApplicationOptions
    {
        public static readonly string DefaultSectionName = "Application";

        public string ServiceBaseUrl { get; set; } = null!;
    }
}
