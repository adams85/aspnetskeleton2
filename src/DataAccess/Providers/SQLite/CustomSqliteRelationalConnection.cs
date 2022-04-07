using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.Sqlite;

internal sealed class CustomSqliteRelationalConnection : SqliteRelationalConnection, IExtendedDbContextTransactionManager
{
    public CustomSqliteRelationalConnection(RelationalConnectionDependencies dependencies, IRawSqlCommandBuilder rawSqlCommandBuilder, IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
        : base(dependencies, rawSqlCommandBuilder, logger)
    {
    }

    bool IExtendedDbContextTransactionManager.SupportsAmbientTransactions => SupportsAmbientTransactions;

    private void CreateCustomObjects()
    {
        var connection = (SqliteConnection)DbConnection;

        connection.CreateCollation(SqliteProperties.DefaultCaseSensitiveCollationName, StringComparer.InvariantCulture.Compare);
        connection.CreateCollation(SqliteProperties.DefaultCaseInsensitiveCollationName, StringComparer.InvariantCultureIgnoreCase.Compare);
    }

    public override bool Open(bool errorsExpected = false)
    {
        if (!base.Open(errorsExpected))
            return false;

        CreateCustomObjects();
        return true;
    }

    public override async Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
    {
        if (!await base.OpenAsync(cancellationToken, errorsExpected).ConfigureAwait(false))
            return false;

        CreateCustomObjects();
        return true;
    }
}
