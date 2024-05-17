using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.PostgreSQL;

internal sealed class CustomNpgsqlRelationalConnection : NpgsqlRelationalConnection, IExtendedDbContextTransactionManager
{
    public CustomNpgsqlRelationalConnection(RelationalConnectionDependencies dependencies, INpgsqlSingletonOptions options)
        : base(dependencies, options) { }

    public CustomNpgsqlRelationalConnection(RelationalConnectionDependencies dependencies, DbDataSource? dataSource)
        : base(dependencies, dataSource) { }

    bool IExtendedDbContextTransactionManager.SupportsAmbientTransactions => SupportsAmbientTransactions;
}
