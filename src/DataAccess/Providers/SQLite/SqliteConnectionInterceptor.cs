using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WebApp.DataAccess.Providers.Sqlite
{
    internal sealed class SqliteConnectionInterceptor : DbConnectionInterceptor
    {
        private static void CreateCustomObjects(SqliteConnection connection)
        {
            connection.CreateCollation(SqliteProperties.DefaultCaseSensitiveCollationName, StringComparer.InvariantCulture.Compare);
            connection.CreateCollation(SqliteProperties.DefaultCaseInsensitiveCollationName, StringComparer.InvariantCultureIgnoreCase.Compare);
        }

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            CreateCustomObjects((SqliteConnection)eventData.Connection);
        }

        public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            CreateCustomObjects((SqliteConnection)eventData.Connection);
            return Task.CompletedTask;
        }
    }
}
