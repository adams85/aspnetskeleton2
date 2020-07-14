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

            builder.Services.AddOptions<MvcOptions>()
                .Configure<IServiceProvider>((options, sp) =>
                {
                    options.ModelValidatorProviders.Add(new DataAnnotationLocalizationAdjuster(
                        sp.GetRequiredService<IValidationAttributeAdapterProvider>(),
                        sp.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(),
                        sp.GetService<IStringLocalizerFactory>()));
                });

            builder.Services.Replace(ServiceDescriptor.Singleton<IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider>());

            builder.Services.AddOptions<MvcDataAnnotationsLocalizationOptions>()
                .Configure<ILoggerFactory>((options, loggerFactory) => options.DataAnnotationLocalizerProvider = (type, stringLocalizerFactory) =>
                    new CompositeStringLocalizer(
                        stringLocalizerFactory,
                        types: new[] { type, typeof(ValidationErrorMessages) },
                        logger: loggerFactory.CreateLogger<CompositeStringLocalizer>()));
        }
    }
}
