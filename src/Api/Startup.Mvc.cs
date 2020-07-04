using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Api.Infrastructure.DataAnnotations;
using WebApp.Api.Infrastructure.ModelBinding;
using WebApp.Service.Infrastructure.Localization;

namespace WebApp.Api
{
    public partial class Startup
    {
        partial void ConfigureMvcPartial(IMvcBuilder builder);

        private void ConfigureMvc(IMvcBuilder builder)
        {
            builder
                .AddJsonOptions(options => options.JsonSerializerOptions.ConfigureApiDefaults())
                .AddProtoBuf();

            builder.ConfigureApiBehaviorOptions(options =>
            {
                // https://stackoverflow.com/questions/54851232/asp-net-core-2-2-problemdetails
                options.SuppressMapClientErrors = true;

                // https://stackoverflow.com/questions/51125569/net-core-2-1-override-automatic-model-validation
                options.SuppressModelStateInvalidFilter = true;
            });

            ConfigureModelServices(builder);

            ConfigureMvcPartial(builder);
        }

        public void ConfigureModelServices(IMvcBuilder builder)
        {
            builder.Services.Replace(ServiceDescriptor.Singleton<IModelMetadataProvider, CustomModelMetadataProvider>());

            ReplaceDataAnnotationsModelValidatorProvider(builder);

            builder.Services.AddOptions<MvcDataAnnotationsLocalizationOptions>()
                .Configure<ILoggerFactory>((options, loggerFactory) => options.DataAnnotationLocalizerProvider = (type, stringLocalizerFactory) =>
                    new CompositeStringLocalizer(new[]
                    {
                        stringLocalizerFactory.Create(type),
                        stringLocalizerFactory.Create(typeof(ValidationErrorMessages))
                    },
                    loggerFactory.CreateLogger<CompositeStringLocalizer>()));
        }

        private static IMvcBuilder ReplaceDataAnnotationsModelValidatorProvider(IMvcBuilder builder)
        {
            builder.Services.AddOptions<MvcOptions>()
                 .Configure<IServiceProvider>((options, sp) =>
                 {
                     var providerCount = options.ModelValidatorProviders.Count;

                     // we need to replace DataAnnotationsModelValidatorProvider with our customized version,
                     // but unfortunataly, that type is internal, so we resort to removing it by name
                     for (int i = providerCount - 1; i >= 0; i--)
                         if (options.ModelValidatorProviders[i].GetType().FullName == "Microsoft.AspNetCore.Mvc.DataAnnotations.DataAnnotationsModelValidatorProvider")
                             options.ModelValidatorProviders.RemoveAt(i);

                     Debug.Assert(providerCount - options.ModelValidatorProviders.Count == 1, "Microsoft.AspNetCore.Mvc.DataAnnotations internals has apparently changed.");

                     options.ModelValidatorProviders.Add(new CustomDataAnnotationsModelValidatorProvider(
                         sp.GetRequiredService<IValidationAttributeAdapterProvider>(),
                         sp.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(),
                         sp.GetService<IStringLocalizerFactory>()));
                 });

            return builder;
        }
    }
}
