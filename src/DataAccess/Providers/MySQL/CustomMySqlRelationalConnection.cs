using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microting.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Microting.EntityFrameworkCore.MySql.Storage.Internal;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.MySQL;

internal sealed class CustomMySqlRelationalConnection : MySqlRelationalConnection, IExtendedDbContextTransactionManager
{
    public CustomMySqlRelationalConnection(RelationalConnectionDependencies dependencies, IMySqlConnectionStringOptionsValidator mySqlConnectionStringOptionsValidator, IMySqlOptions mySqlSingletonOptions)
        : base(dependencies, mySqlConnectionStringOptionsValidator, mySqlSingletonOptions) { }

    public CustomMySqlRelationalConnection(RelationalConnectionDependencies dependencies, IMySqlConnectionStringOptionsValidator mySqlConnectionStringOptionsValidator, DbDataSource dataSource)
        : base(dependencies, mySqlConnectionStringOptionsValidator, dataSource) { }

    bool IExtendedDbContextTransactionManager.SupportsAmbientTransactions => SupportsAmbientTransactions;
}
