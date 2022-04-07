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

internal class CommandContext : IDisposable, IAsyncDisposable
{
    private readonly IServiceScope _serviceScope;

    private CommandContext(ICommand command, IServiceScope serviceScope)
    {
        Command = command;
        CommandType = command.GetType();

        _serviceScope = serviceScope;
    }

    public CommandContext(ICommand command, IServiceScopeFactory serviceScopeFactory)
        : this(command, serviceScopeFactory.CreateScope()) { }

    public CommandContext(ICommand command, IServiceProvider serviceProvider)
        : this(command, serviceProvider.CreateScope()) { }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return AsyncDisposableAdapter.From(_serviceScope).DisposeAsync();
    }

    public IServiceProvider ScopedServices => _serviceScope.ServiceProvider;

    public ICommand Command { get; }
    public Type CommandType { get; }

    private IExecutionContextAccessor? _executionContextAccessor;
    public virtual OperationExecutionContext ExecutionContext =>
        LazyInitializer.EnsureInitialized(ref _executionContextAccessor, () => ScopedServices.GetRequiredService<IExecutionContextAccessor>())!.ExecutionContext;

    private IDictionary<object, object>? _properties;
    public virtual IDictionary<object, object> Properties => LazyInitializer.EnsureInitialized(ref _properties, () => new Dictionary<object, object>())!;

    public virtual WritableDataContext CreateDbContext() => ScopedServices.GetRequiredService<IDbContextFactory<WritableDataContext>>().CreateDbContext();
}
