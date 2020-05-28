using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using WebApp.Core;

namespace WebApp.Api
{
    public partial class Program
    {
        public static readonly ApplicationArchitecture Architecture = ApplicationArchitecture.Monolithic;

        static partial void ConfigureAppConfigurationPartial(HostBuilderContext context, IConfigurationBuilder builder, ref int index)
        {
            const string serviceSettingsTag = "Service";

            var env = context.HostingEnvironment;

            var fileProvider = builder.GetFileProvider();
            if (!fileProvider.GetFileInfo($"appsettings.{serviceSettingsTag}.json").Exists)
                fileProvider = new PhysicalFileProvider(AppContext.BaseDirectory);

            // adding shared configuration files which are linked from Service.Host project
            builder.Sources.Insert(index++, new JsonConfigurationSource { FileProvider = fileProvider, Path = $"appsettings.{serviceSettingsTag}.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { FileProvider = fileProvider, Path = $"appsettings.{serviceSettingsTag}.{env.EnvironmentName}.json", Optional = true, ReloadOnChange = true });
        }
    }
}
