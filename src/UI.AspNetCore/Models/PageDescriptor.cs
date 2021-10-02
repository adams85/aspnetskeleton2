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
        private static readonly ConcurrentDictionary<Type, PageDescriptor> s_pageDescriptorCache = new ConcurrentDictionary<Type, PageDescriptor>();

        public static PageDescriptor Get(Type providerType) => s_pageDescriptorCache.GetOrAdd(providerType, providerType =>
        {
            if (!providerType.HasInterface(typeof(IPageDescriptorProvider)))
                throw new ArgumentException($"Type does not implement {typeof(IPageDescriptorProvider)}.", nameof(providerType));

            var staticProviderAttribute = providerType.GetCustomAttribute<StaticPageDescriptorProviderAttribute>(inherit: true);

            if (staticProviderAttribute != null)
            {
                const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;

                var field = providerType.GetField(staticProviderAttribute.MemberName, bindingFlags);
                if (field != null)
                    return (PageDescriptor)field.GetValue(null)!;

                var property = providerType.GetProperty(staticProviderAttribute.MemberName, bindingFlags);
                if (property != null)
                    return (PageDescriptor)property.GetValue(null)!;

                throw new ArgumentException($"Type does not have a static member named '{staticProviderAttribute.MemberName}'.", nameof(providerType));
            }
            else
            {
                ConstructorInfo? ctor;
                if (providerType.IsAbstract || (ctor = providerType.GetConstructor(Type.EmptyTypes)) == null)
                    throw new ArgumentException("Type is abstract or does not have a parameterless constructor.", nameof(providerType));

                var provider = (IPageDescriptorProvider)ctor.Invoke(Array.Empty<object?>());
                return provider.PageDescriptor;
            }
        });

        public static PageDescriptor Get<TProvider>() where TProvider : IPageDescriptorProvider => Get(typeof(TProvider));

        public PageDescriptor()
        {
            IsAccessAllowedAsync = httpContext =>
                httpContext.RequestServices.GetRequiredService<IPageAuthorizationHelper>().CheckAccessAllowedAsync(httpContext, PageName, AreaName);
        }

        public abstract string PageName { get; }
        public virtual string AreaName => string.Empty;

        public virtual Func<HttpContext, Task<AuthorizationPolicy>>? GetAuthorizationPolicyAsync => null;

        public virtual Func<HttpContext, Task<bool>> IsAccessAllowedAsync { get; }

        public virtual string? LayoutName => null;

        public abstract Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; }
    }
}
