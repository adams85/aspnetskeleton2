﻿using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp.Api.Infrastructure.Swagger;

namespace WebApp.Api
{
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
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.<br/>" +
                      "Enter 'Bearer &lt;token&gt;' in the text input below.<br/>" +
                      "Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });

                var securitySchemeRef = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                };

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [securitySchemeRef] = Array.Empty<string>()
                });

                // uncomment the following line to enable polymorphic schema generation
                //options.GeneratePolymorphicSchemas(ApiContractSerializer.MetadataProvider.GetSubTypes, _ => ApiContractSerializer.JsonTypePropertyName);
            });

            services.Replace(ServiceDescriptor.Transient<IDataContractResolver, CustomJsonSerializerDataContractResolver>());
        }

        private void ConfigureSwagger(IApplicationBuilder app)
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "doc/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "doc";
                options.SwaggerEndpoint($"{ApiExplorerGroupConvention.DefaultGroupName}/swagger.json", DefaultSwaggerDocName);
            });
        }
    }
}
