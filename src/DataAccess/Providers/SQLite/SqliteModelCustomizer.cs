using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.Sqlite;

internal sealed class SqliteModelCustomizer : RelationalModelCustomizer
{
    private readonly string _caseSensitiveCollation;
    private readonly string _caseInsensitiveCollation;

    public SqliteModelCustomizer(IDbProperties dbProperties, ModelCustomizerDependencies dependencies) : base(dependencies)
    {
        if (dbProperties == null)
            throw new ArgumentNullException(nameof(dbProperties));

        _caseSensitiveCollation = dbProperties.CaseSensitiveCollation;
        _caseInsensitiveCollation = dbProperties.CaseInsensitiveCollation;
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.PropertyInfo != null)
                {
                    if (Type.GetTypeCode(property.ClrType) == TypeCode.String && property.GetColumnType() == null)
                    {
                        var annotation = property.FindAnnotation(ModelBuilderExtensions.CaseInsensitiveAnnotationKey);
                        var caseInsensitive = annotation != null || property.PropertyInfo.GetCustomAttributes<CaseInsensitiveAttribute>().Any();
                        property.SetColumnType("TEXT COLLATE " + (caseInsensitive ? _caseInsensitiveCollation : _caseSensitiveCollation));
                    }

                    if (Type.GetTypeCode(Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == TypeCode.Decimal)
                        property.SetColumnType("DECIMAL(19, 4)");
                }
            }
        }
    }
}
