using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace WebApp.UI.Infrastructure
{
    public sealed class RuntimeCompilationAwareValueCache<T> : SingleValueCache<T> where T : class
    {
        public RuntimeCompilationAwareValueCache(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
            : base(
                (actionDescriptorCollectionProvider ?? throw new ArgumentNullException(nameof(actionDescriptorCollectionProvider))) is ActionDescriptorCollectionProvider provider ?
                provider.GetChangeToken :
                (Func<IChangeToken>?)null)
        {
        }
    }
}
