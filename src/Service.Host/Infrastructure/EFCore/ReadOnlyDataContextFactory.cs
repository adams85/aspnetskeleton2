using System;
using Microsoft.EntityFrameworkCore.Design;
using WebApp.DataAccess;

namespace WebApp.Service.Host.Infrastructure.EFCore
{
    internal class ReadOnlyDataContextFactory : IDesignTimeDbContextFactory<ReadOnlyDataContext>
    {
        public ReadOnlyDataContext CreateDbContext(string[] args)
        {
            throw new InvalidOperationException($"Use {nameof(WritableDataContext)} for EF Core database migrations.");
        }
    }
}
