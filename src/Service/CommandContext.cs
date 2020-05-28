﻿using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using WebApp.DataAccess;

namespace WebApp.Service
{
    internal class CommandContext : IDisposable
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

        public IServiceProvider ScopedServices => _serviceScope.ServiceProvider;

        public ICommand Command { get; }
        public Type CommandType { get; }

        private DataContext? _dbContext;
        public virtual DataContext DbContext => LazyInitializer.EnsureInitialized(ref _dbContext, () => ScopedServices.GetRequiredService<WritableDataContext>())!;

        private IDictionary<object, object>? _properties;
        public virtual IDictionary<object, object> Properties => LazyInitializer.EnsureInitialized(ref _properties, () => new Dictionary<object, object>())!;
    }
}
