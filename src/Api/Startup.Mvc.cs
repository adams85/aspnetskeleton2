using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Api.Infrastructure.DataAnnotations;
using WebApp.Api.Infrastructure.ModelBinding;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Settings;

namespace WebApp.Api;

public partial class Startup
{
    partial void ConfigureMvcPartial(IMvcBuilder builder);

    private void ConfigureMvc(IMvcBuilder builder)
    {
        ConfigureModelBinding(builder);
        ConfigureModelValidation(builder);

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

        ConfigureMvcPartial(builder);
    }

    public void ConfigureModelBinding(IMvcBuilder builder)
    {
        builder.Services.AddOptions<MvcOptions>()
            .Configure<ISettingsProvider>((options, settingsProvider) =>
            {
                options.ModelMetadataDetailsProviders.Add(new DataContractMetadataDetailsProvider());

                var index = options.ModelBinderProviders.FindIndex(provider => provider is ComplexObjectModelBinderProvider);
                Debug.Assert(index >= 0, "Microsoft.AspNetCore.Mvc.MvcOptions.ModelBinderProviders defaults have apparently changed.");
                options.ModelBinderProviders.Insert(index, new ListQueryModelBinderProvider(options.ModelBinderProviders[index], settingsProvider));
            });
    }

    public void ConfigureModelValidation(IMvcBuilder builder)
    {
        builder.Services.ReplaceLast(ServiceDescriptor.Singleton<IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider>());

        builder.Services.AddOptions<MvcOptions>()
            .Configure<IValidationAttributeAdapterProvider, IOptions<MvcDataAnnotationsLocalizationOptions>, IStringLocalizerFactory>(
                (options, validationAttributeAdapterProvider, localizationOptions, stringLocalizerFactory) =>
                    options.ModelValidatorProviders.Add(new DataAnnotationsLocalizationAdjuster(validationAttributeAdapterProvider, localizationOptions, stringLocalizerFactory)));

        builder.AddDataAnnotationsLocalization();

        builder.Services.AddOptions<MvcDataAnnotationsLocalizationOptions>()
            .Configure<ILoggerFactory>((options, loggerFactory) => options.DataAnnotationLocalizerProvider = (type, stringLocalizerFactory) =>
                new CompositeStringLocalizer(
                    stringLocalizerFactory,
                    types: new[] { type, typeof(ValidationErrorMessages) },
                    logger: loggerFactory.CreateLogger<CompositeStringLocalizer>()));
    }
}
