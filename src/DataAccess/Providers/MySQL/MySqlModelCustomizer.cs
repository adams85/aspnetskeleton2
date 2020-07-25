using System;
using System.ComponentModel.DataAnnotations.Schema;
using Karambolo.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Extensions;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.MySQL
{
    internal sealed class MySqlModelCustomizer : RelationalModelCustomizer
    {
        private readonly IDbProperties _dbProperties;

        public MySqlModelCustomizer(ModelCustomizerDependencies dependencies, IDbProperties dbProperties) : base(dependencies)
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
                            var caseInsensitive = annotation != null || property.PropertyInfo.HasAttribute<CaseInsensitiveAttribute>();
                            property.SetCollation(caseInsensitive ? _dbProperties.CaseInsensitiveCollation :  _dbProperties.CaseSensitiveCollation);
                        }
                    }
        }
    }
}
