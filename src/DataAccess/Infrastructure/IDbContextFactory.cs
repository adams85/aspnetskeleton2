namespace Microsoft.EntityFrameworkCore
{
    // based on: https://github.com/dotnet/efcore/blob/v6.0.0-rc.1.21452.10/src/EFCore/IDbContextFactory.cs
    // TODO: this type will become unnecessary and should be removed after upgrading to .NET 5+
    public interface IDbContextFactory<TContext>
        where TContext : PooledDbContext
    {
        TContext CreateDbContext();
    }
}
