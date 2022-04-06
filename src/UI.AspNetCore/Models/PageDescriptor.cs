using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using WebApp.UI.Infrastructure.Security;

namespace WebApp.UI.Models
{
    public abstract class PageDescriptor
    {
        private static readonly MethodInfo s_getMethodDefinition = new Func<PageDescriptor>(Get<Pages.IndexModel>).Method.GetGenericMethodDefinition();
        private static readonly ConcurrentDictionary<Type, Func<PageDescriptor>> s_pageDescriptorAccessorCache = new ConcurrentDictionary<Type, Func<PageDescriptor>>();

        public static PageDescriptor Get(Type providerType) => s_pageDescriptorAccessorCache.GetOrAdd(providerType, providerType =>
        {
            if (!providerType.HasInterface(typeof(IPageDescriptorProvider)))
                throw new ArgumentException($"Type does not implement {typeof(IPageDescriptorProvider)}.", nameof(providerType));
            
            var getMethod = s_getMethodDefinition.MakeGenericMethod(providerType);
            return (Func<PageDescriptor>)Delegate.CreateDelegate(typeof(Func<PageDescriptor>), getMethod);
        })();

        public static PageDescriptor Get<TProvider>() where TProvider : IPageDescriptorProvider => TProvider.PageDescriptorStatic;

        protected PageDescriptor() { }

        public abstract string PageName { get; }
        public virtual string AreaName => string.Empty;

        public virtual Task<bool> IsAccessAllowedAsync(HttpContext httpContext) =>
            httpContext.RequestServices.GetRequiredService<IPageAuthorizationHelper>().CheckAccessAllowedAsync(httpContext, PageName, AreaName);

        public virtual string? LayoutName => null;

        public abstract LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer htmlLocalizer);
    }
}
