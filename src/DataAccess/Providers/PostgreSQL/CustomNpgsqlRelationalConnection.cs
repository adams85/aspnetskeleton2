using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.PostgreSQL;

internal sealed class CustomNpgsqlRelationalConnection : NpgsqlRelationalConnection, IExtendedDbContextTransactionManager
{
    public CustomNpgsqlRelationalConnection(RelationalConnectionDependencies dependencies, DbDataSource? dataSource)
        : base(dependencies, dataSource) { }

    public CustomNpgsqlRelationalConnection(RelationalConnectionDependencies dependencies, NpgsqlDataSourceManager dataSourceManager, IDbContextOptions options)
        : base(dependencies, dataSourceManager, options) { }

    bool IExtendedDbContextTransactionManager.SupportsAmbientTransactions => SupportsAmbientTransactions;
}
