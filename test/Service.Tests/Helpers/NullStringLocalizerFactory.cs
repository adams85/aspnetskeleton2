using System;
using Microsoft.Extensions.Localization;

namespace WebApp.Service.Tests.Helpers
{
    public class NullStringLocalizerFactory : IStringLocalizerFactory
    {
        public static readonly NullStringLocalizerFactory Instance = new NullStringLocalizerFactory();

        private NullStringLocalizerFactory() { }

        public IStringLocalizer Create(Type resourceSource) => NullStringLocalizer.Instance;

        public IStringLocalizer Create(string baseName, string location) => NullStringLocalizer.Instance;
    }
}
