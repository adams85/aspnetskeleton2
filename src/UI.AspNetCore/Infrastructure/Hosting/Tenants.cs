using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Karambolo.Common;

namespace WebApp.UI.Infrastructure.Hosting
{
    public sealed class Tenants : IReadOnlyCollection<Tenant>, IDisposable, IAsyncDisposable
    {
        private readonly IReadOnlyDictionary<string, Tenant> _tenants;

        public Tenants(IEnumerable<Tenant> tenants)
        {
            _tenants = tenants.ToDictionary(tenant => tenant.Id, CachedDelegates.Identity<Tenant>.Func);
            MainBranchTenant = tenants.FirstOrDefault(tenant => tenant.BranchPredicate == null);
        }

        public Tenants(params Tenant[] tenants) : this((IEnumerable<Tenant>)tenants) { }

        public void Dispose()
        {
            foreach (var tenant in this)
                tenant.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var tenant in this)
                await tenant.DisposeAsync();
        }

        public Tenant? this[string id] => _tenants.TryGetValue(id, out var tenant) ? tenant : null;

        public Tenant? MainBranchTenant { get; }

        public int Count => _tenants.Count;

        public IEnumerator<Tenant> GetEnumerator() => _tenants.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void InitializeServices(AutofacServiceProvider rootServices)
        {
            foreach (var tenant in this)
                tenant.InitializeServices(rootServices);
        }
    }
}
