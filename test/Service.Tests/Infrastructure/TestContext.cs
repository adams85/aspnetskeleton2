﻿using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Core.Helpers;
using WebApp.DataAccess;

namespace WebApp.Service.Tests.Infrastructure
{
    /// <summary>
    /// Encapsulates context for test cases. Context is essentially a configured IoC container.
    /// (For complete isolation a dedicated IoC container is created for each test case.)
    /// </summary>
    public class TestContext : IDisposable, IAsyncDisposable
    {
        public static readonly string BasePath = GetBasePath();

        private static string GetBasePath()
        {
            // https://stackoverflow.com/questions/23515736/how-to-refer-to-test-files-from-xunit-tests-in-visual-studio
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase!);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            return Path.GetDirectoryName(codeBasePath) ?? string.Empty;
        }

        private readonly IServiceProvider _services;
        private readonly IServiceScope _scope;

        public TestContext(Guid id, IServiceProvider services)
        {
            Id = id;
            _services = services;
            _scope = services.CreateScope();
        }

        public void Dispose()
        {
            _scope.Dispose();

            if (_services is IDisposable servicesDisposable)
                servicesDisposable.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await AsyncDisposableAdapter.From(_scope).DisposeAsync();

            if (_services is IDisposable servicesDisposable)
                await AsyncDisposableAdapter.From(servicesDisposable).DisposeAsync();
        }

        public Guid Id { get; }

        public IServiceProvider Services => _scope.ServiceProvider;

        /// <summary>
        /// Creates a read-only EF data context for data access (in case EF Core services were added to the container during configuration).
        /// </summary>
        public ReadOnlyDataContext CreateReadOnlyDbContext() => Services.GetRequiredService<IDbContextFactory<ReadOnlyDataContext>>().CreateDbContext();

        /// <summary>
        /// Creates a writable EF data context for data access (in case EF Core services were added to the container during configuration).
        /// </summary>
        public WritableDataContext CreateWritableDbContext() => Services.GetRequiredService<IDbContextFactory<WritableDataContext>>().CreateDbContext();
    }
}
