using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace WebApp.DataAccess.Providers.SqlServer
{
    internal sealed class SqlServerModelCustomizer : RelationalModelCustomizer
    {
        public SqlServerModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies) { }

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
                            if (annotation == null)
                                property.AddAnnotation(ModelBuilderExtensions.CaseInsensitiveAnnotationKey, property.PropertyInfo.GetCustomAttributes<CaseInsensitiveAttribute>().Any());
                        }

                        if (Type.GetTypeCode(Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == TypeCode.Decimal && property.GetColumnType() == null)
                            property.SetColumnType("MONEY");
                    }
        }
    }
}
