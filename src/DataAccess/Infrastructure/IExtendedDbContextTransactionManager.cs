using Microsoft.EntityFrameworkCore.Storage;

namespace WebApp.DataAccess.Infrastructure
{
    internal interface IExtendedDbContextTransactionManager : IDbContextTransactionManager
    {
        bool SupportsAmbientTransactions { get; }
    }
}
