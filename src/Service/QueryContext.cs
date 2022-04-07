using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Core.Helpers;
using WebApp.DataAccess;
using WebApp.Service.Infrastructure;

namespace WebApp.Service;

internal partial class QueryContext : IDisposable, IAsyncDisposable
{
    private readonly IServiceScope _serviceScope;

    private QueryContext(IQuery query, IServiceScope serviceScope)
    {
        Query = query;
        QueryType = query.GetType();
        ResultType = GetResultType(QueryType);

        _serviceScope = serviceScope;
    }

    public QueryContext(IQuery query, IServiceScopeFactory serviceScopeFactory)
        : this(query, serviceScopeFactory.CreateScope()) { }

    public QueryContext(IQuery query, IServiceProvider serviceProvider)
        : this(query, serviceProvider.CreateScope()) { }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return AsyncDisposableAdapter.From(_serviceScope).DisposeAsync();
    }

    public IServiceProvider ScopedServices => _serviceScope.ServiceProvider;

    public IQuery Query { get; }
    public Type QueryType { get; }
    public Type ResultType { get; }

    private IExecutionContextAccessor? _executionContextAccessor;
    public virtual OperationExecutionContext ExecutionContext =>
        LazyInitializer.EnsureInitialized(ref _executionContextAccessor, () => ScopedServices.GetRequiredService<IExecutionContextAccessor>())!.ExecutionContext;

    private IDictionary<object, object>? _properties;
    public virtual IDictionary<object, object> Properties => LazyInitializer.EnsureInitialized(ref _properties, () => new Dictionary<object, object>())!;

    public virtual ReadOnlyDataContext CreateDbContext() => ScopedServices.GetRequiredService<IDbContextFactory<ReadOnlyDataContext>>().CreateDbContext();
}
