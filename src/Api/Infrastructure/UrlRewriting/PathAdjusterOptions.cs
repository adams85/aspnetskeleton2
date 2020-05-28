using System.Collections.Generic;

namespace WebApp.Api.Infrastructure.UrlRewriting
{
    public class PathAdjusterOptions
    {
        public static readonly string DefaultSectionName = "UrlRewriting";

        public IList<PathAdjustment>? PathAdjustments { get; set; }
    }
}
