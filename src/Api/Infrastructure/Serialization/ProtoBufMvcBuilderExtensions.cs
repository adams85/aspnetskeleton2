using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using WebApp.Api.Infrastructure.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

public static class ProtoBufMvcBuilderExtensions
{
    private static void ConfigureMvcOptions(ProtoBufFormatterOptions options, MvcOptions mvcOptions, ILoggerFactory loggerFactory)
    {
        mvcOptions.InputFormatters.Add(new ProtoBufInputFormatter(options, mvcOptions, loggerFactory.CreateLogger<ProtoBufInputFormatter>()));
        mvcOptions.OutputFormatters.Add(new ProtoBufOutputFormatter(options, mvcOptions));
    }

    public static IMvcCoreBuilder AddProtoBuf(this IMvcCoreBuilder builder, Action<ProtoBufFormatterOptions>? setupAction = null)
    {
        var options = new ProtoBufFormatterOptions();
        setupAction?.Invoke(options);

        foreach (var extension in options.SupportedExtensions)
        {
            foreach (var contentType in options.SupportedContentTypes)
                builder.AddFormatterMappings(m => m.SetMediaTypeMappingForFormat(extension, new MediaTypeHeaderValue(contentType)));
        }

        builder.Services.AddOptions<MvcOptions>()
            .Configure<ILoggerFactory>((mvcOptions, loggerFactory) => ConfigureMvcOptions(options, mvcOptions, loggerFactory));

        return builder;
    }

    public static IMvcBuilder AddProtoBuf(this IMvcBuilder builder, Action<ProtoBufFormatterOptions>? setupAction = null)
    {
        var options = new ProtoBufFormatterOptions();
        setupAction?.Invoke(options);

        foreach (var extension in options.SupportedExtensions)
        {
            foreach (var contentType in options.SupportedContentTypes)
                builder.AddFormatterMappings(m => m.SetMediaTypeMappingForFormat(extension, new MediaTypeHeaderValue(contentType)));
        }

        builder.Services.AddOptions<MvcOptions>()
            .Configure<ILoggerFactory>((mvcOptions, loggerFactory) => ConfigureMvcOptions(options, mvcOptions, loggerFactory));

        return builder;
    }
}
