using System;

namespace WebApp.UI
{
    [Flags]
    public enum ResponseOptions
    {
        None = 0,
        Minify = 0x1,
        Cache = 0x2,
    }

    public class UIOptions
    {
        public const string DefaultSectionName = "Application:UI";

        public static readonly TimeSpan DefaultCacheHeaderMaxAge = TimeSpan.FromDays(7);

        public bool EnableRazorRuntimeCompilation { get; set; }

        public StaticFileOptions StaticFiles { get; } = new StaticFileOptions();
        public BundleOptions Bundles { get; } = new BundleOptions();
        public ViewOptions Views { get; } = new ViewOptions();
        public bool EnableResponseCompression { get; set; }
        public bool EnableStatusCodePages { get; set; }

        public class StaticFileOptions
        {
            public bool EnableResponseCaching { get; set; }
            public TimeSpan? CacheHeaderMaxAge { get; set; }
        }

        public class BundleOptions
        {
            public bool EnableResponseMinification { get; set; }
            public bool EnableResponseCaching { get; set; }
            public TimeSpan? CacheHeaderMaxAge { get; set; }
            public bool UsePersistentCache { get; set; }
        }

        public class ViewOptions
        {
            public bool EnableResponseMinification { get; set; }
            public bool EnableResponseCaching { get; set; }
            public TimeSpan? CacheHeaderMaxAge { get; set; }
        }
    }
}
