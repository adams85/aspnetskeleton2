using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal;

namespace WebApp.DataAccess.Providers.SqlServer
{
    // https://github.com/aspnet/EntityFrameworkCore/issues/10258#issuecomment-343605538"/>
    internal sealed class CustomSqlServerMigrationsAnnotationProvider : SqlServerMigrationsAnnotationProvider
    {
        public CustomSqlServerMigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
            : base(dependencies) { }

        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            var annotations = base.For(property);

            if (Type.GetTypeCode(property.ClrType) == TypeCode.String)
            {
                var annotation = property.FindAnnotation(ModelBuilderExtensions.CaseInsensitiveAnnotationKey);
                if (annotation != null)
                    annotations = annotations.Concat(new[] { annotation });
            }

            return annotations;
        }
    }
}
