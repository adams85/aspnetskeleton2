using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.PostgreSQL
{
    internal sealed class CustomNpgsqlRelationalConnection : NpgsqlRelationalConnection, IExtendedDbContextTransactionManager
    {
        public CustomNpgsqlRelationalConnection(RelationalConnectionDependencies dependencies)
            : base(dependencies) { }

        bool IExtendedDbContextTransactionManager.SupportsAmbientTransactions => SupportsAmbientTransactions;
    }
}
