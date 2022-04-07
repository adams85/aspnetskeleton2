using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace WebApp.DataAccess.Migrations;

public sealed partial class CustomDbObjects
{
    public CustomDbObjects(IModel targetModel, string activeProvider)
    {
        TargetModel = targetModel;
        ActiveProvider = activeProvider;
    }

    public IModel TargetModel { get; }
    public string ActiveProvider { get; }

    public IEnumerable<MigrationOperation> GetAllDbObjectsOperations(bool dropIfExists, bool create) =>
        GetTriggerOperations(dropIfExists, create)
            .Concat(GetViewOperations(dropIfExists, create))
            .Concat(GetStoredProcOperations(dropIfExists, create))
            .Concat(GetFunctionOperations(dropIfExists, create));
}
