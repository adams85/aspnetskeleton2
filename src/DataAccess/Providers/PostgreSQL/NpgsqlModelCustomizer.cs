using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.PostgreSQL;

internal sealed class NpgsqlModelCustomizer : RelationalModelCustomizer
{
    private readonly string _caseSensitiveCollation;
    private readonly string _caseInsensitiveCollation;

    public NpgsqlModelCustomizer(IDbProperties dbProperties, ModelCustomizerDependencies dependencies) : base(dependencies)
    {
        if (dbProperties == null)
            throw new ArgumentNullException(nameof(dbProperties));

        _caseSensitiveCollation = dbProperties.CaseSensitiveCollation;
        _caseInsensitiveCollation = dbProperties.CaseInsensitiveCollation;
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        // https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html?tabs=data-annotations
        modelBuilder.HasCollation(nameof(IDbProperties.CaseSensitiveCollation), locale: _caseSensitiveCollation, provider: "icu", deterministic: false);
        modelBuilder.HasCollation(nameof(IDbProperties.CaseInsensitiveCollation), locale: _caseInsensitiveCollation, provider: "icu", deterministic: false);

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
                        // https://www.npgsql.org/efcore/release-notes/7.0.html#obsoleted-default-column-collations
                        property.SetCollation(caseInsensitive ? _caseInsensitiveCollation : _caseSensitiveCollation);
                    }
                }
            }
        }
    }
}
