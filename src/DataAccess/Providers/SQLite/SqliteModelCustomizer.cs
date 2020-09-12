using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.Sqlite
{
    internal sealed class SqliteModelCustomizer : RelationalModelCustomizer
    {
        private readonly IDbProperties _dbProperties;

        public SqliteModelCustomizer(ModelCustomizerDependencies dependencies, IDbProperties dbProperties) : base(dependencies)
        {
            _dbProperties = dbProperties ?? throw new ArgumentNullException(nameof(dbProperties));
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                foreach (var property in entityType.GetProperties())
                    if (property.PropertyInfo != null)
                    {
                        if (Type.GetTypeCode(property.ClrType) == TypeCode.String && property.GetColumnType() == null)
                        {
                            var annotation = property.FindAnnotation(ModelBuilderExtensions.CaseInsensitiveAnnotationKey);
                            var caseInsensitive = annotation != null || property.PropertyInfo.GetCustomAttributes<CaseInsensitiveAttribute>().Any();
                            property.SetColumnType(caseInsensitive ? "TEXT COLLATE " + _dbProperties.CaseInsensitiveCollation : "TEXT COLLATE " + _dbProperties.CaseSensitiveCollation);
                        }

                        if (Type.GetTypeCode(Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == TypeCode.Decimal)
                            property.SetColumnType("DECIMAL(19, 4)");
                    }
        }
    }
}
