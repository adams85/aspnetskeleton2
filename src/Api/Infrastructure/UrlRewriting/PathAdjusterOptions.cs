using System.Collections.Generic;

namespace WebApp.Api.Infrastructure.UrlRewriting
{
    public class PathAdjusterOptions
    {
        public static readonly string DefaultSectionName = "UrlRewriting";

        public List<PathAdjustment> PathAdjustments { get; } = new List<PathAdjustment>();
    }
}
