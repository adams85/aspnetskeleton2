using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace WebApp.DataAccess.Migrations
{
    public partial class CustomDbObjects
    {
        public IEnumerable<MigrationOperation> GetViewOperations(bool dropIfExists, bool create)
        {
            return Enumerable.Empty<MigrationOperation>();
        }
    }
}
