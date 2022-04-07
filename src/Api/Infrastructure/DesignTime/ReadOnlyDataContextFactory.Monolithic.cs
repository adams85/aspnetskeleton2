using System;
using Microsoft.EntityFrameworkCore.Design;
using WebApp.DataAccess;

namespace WebApp.Api.Infrastructure.DesignTime;

internal class ReadOnlyDataContextFactory : IDesignTimeDbContextFactory<ReadOnlyDataContext>
{
    public ReadOnlyDataContext CreateDbContext(string[] args)
    {
        throw new InvalidOperationException($"Use {nameof(WritableDataContext)} for EF Core database migrations.");
    }
}
