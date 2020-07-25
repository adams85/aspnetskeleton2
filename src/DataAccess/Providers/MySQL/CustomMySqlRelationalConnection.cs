using Microsoft.EntityFrameworkCore.Storage;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.MySQL
{
    internal sealed class CustomMySqlRelationalConnection : MySqlRelationalConnection, IExtendedDbContextTransactionManager
    {
        public CustomMySqlRelationalConnection(RelationalConnectionDependencies dependencies)
            : base(dependencies) { }

        bool IExtendedDbContextTransactionManager.SupportsAmbientTransactions => SupportsAmbientTransactions;
    }
}
