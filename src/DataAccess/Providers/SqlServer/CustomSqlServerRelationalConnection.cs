using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.SqlServer
{
    internal sealed class CustomSqlServerRelationalConnection : SqlServerConnection, IExtendedDbContextTransactionManager
    {
        public CustomSqlServerRelationalConnection(RelationalConnectionDependencies dependencies)
            : base(dependencies) { }

        bool IExtendedDbContextTransactionManager.SupportsAmbientTransactions => SupportsAmbientTransactions;
    }
}
