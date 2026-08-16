using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp.Api.Infrastructure.Security;
using WebApp.Api.Infrastructure.Swagger;
using WebApp.Core.Helpers;

namespace WebApp.Api;

public partial class Startup
{
    private static string DefaultSwaggerDocName => Program.ApplicationName + " - " + ApiExplorerGroupConvention.DefaultGroupName;

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(ApiExplorerGroupConvention.DefaultGroupName, new OpenApiInfo
            {
                Title = DefaultSwaggerDocName,
                Version = "v1"
            });

            var filePath = Path.ChangeExtension(typeof(Program).Assembly.Location, ".xml");
            options.IncludeXmlComments(filePath);

            // https://stackoverflow.com/questions/56234504/migrating-to-swashbuckle-aspnetcore-version-5
            // https://stackoverflow.com/questions/43447688/setting-up-swagger-asp-net-core-using-the-authorization-headers-bearer
            options.AddSecurityDefinition(ApiAuthenticationSchemes.JwtBearer, new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = ApiAuthenticationSchemes.JwtBearer,
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(ApiAuthenticationSchemes.JwtBearer, document)] = new List<string>()
                }
            );

            options.CustomSchemaIds(type => type.FullNameWithoutAssemblyDetails());

            options.UseOneOfForPolymorphism();
            options.SelectSubTypesUsing(type => ApiContractSerializer.MetadataProvider.GetSubTypes(type)
                .Select(kvp => new CustomJsonSerializerDataContractResolver.SubType(kvp.Key, kvp.Value))
                .ToArray());
            options.SelectDiscriminatorNameUsing(_ => ApiContractSerializer.JsonTypeDiscriminatorPropertyName);
            // NOTE: options.SelectDiscriminatorValueUsing() doesn't seem to work,
            // so discriminator values are set in CustomJsonSerializerDataContractResolver.
        });

        services.ReplaceLast(ServiceDescriptor.Transient<ISerializerDataContractResolver, CustomJsonSerializerDataContractResolver>());
    }

    private void ConfigureSwagger(IApplicationBuilder app)
    {
        app.UseSwagger(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            options.RouteTemplate = "doc/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "doc";
            options.SwaggerEndpoint($"{ApiExplorerGroupConvention.DefaultGroupName}/swagger.json", DefaultSwaggerDocName);
        });
    }
}
