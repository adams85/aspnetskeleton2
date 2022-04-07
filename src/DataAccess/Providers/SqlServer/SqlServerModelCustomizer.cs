using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.SqlServer;

internal sealed class SqlServerModelCustomizer : RelationalModelCustomizer
{
    private readonly string _caseSensitiveCollation;
    private readonly string _caseInsensitiveCollation;

    public SqlServerModelCustomizer(IDbProperties dbProperties, ModelCustomizerDependencies dependencies) : base(dependencies)
    {
        if (dbProperties == null)
            throw new ArgumentNullException(nameof(dbProperties));

        _caseSensitiveCollation = dbProperties.CaseSensitiveCollation;
        _caseInsensitiveCollation = dbProperties.CaseInsensitiveCollation;
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        modelBuilder.UseCollation(_caseSensitiveCollation);

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
                        if (caseInsensitive)
                            property.SetCollation(_caseInsensitiveCollation);
                    }

                    if (Type.GetTypeCode(Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == TypeCode.Decimal && property.GetColumnType() == null)
                        property.SetColumnType("MONEY");
                }
            }
        }
    }
}
