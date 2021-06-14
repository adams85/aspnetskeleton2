using System;
using Microsoft.AspNetCore.Mvc.Localization;

#if SERVICE_HOST
namespace WebApp.Service.Host.Infrastructure.Localization
#else
namespace WebApp.Api.Infrastructure.Localization
#endif
{
    public sealed class NullHtmlLocalizerFactory : IHtmlLocalizerFactory
    {
        public static readonly NullHtmlLocalizerFactory Instance = new NullHtmlLocalizerFactory();

        private NullHtmlLocalizerFactory() { }

        public IHtmlLocalizer Create(Type resourceSource) => NullHtmlLocalizer.Instance;

        public IHtmlLocalizer Create(string baseName, string location) => NullHtmlLocalizer.Instance;
    }
}
