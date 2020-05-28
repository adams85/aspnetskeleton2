using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApp.DataAccess;
using WebApp.Service;
using WebApp.Service.Infrastructure.Database;
using WebApp.Service.Mailing;

#if !DISTRIBUTED
namespace WebApp.Api
#else
namespace WebApp.Service.Host
#endif
{
    public partial class Startup
    {
        partial void ConfigureServiceLayerImmediateOptionsPartial(IServiceCollection services)
        {
            var dbOptionsSection = Configuration.GetSection(DbOptions.DefaultSectionName);
            services.Configure<DbOptions>(dbOptionsSection);

            services.AddOptions<DataAccessOptions>()
                .Configure<IOptions<DbOptions>>((options, dbOptions) =>
                {
                    options.Database = dbOptions.Value;
                    options.EnableSqlLogging = dbOptionsSection.GetValue(nameof(options.EnableSqlLogging), defaultValue: false);
                });
        }

        partial void ConfigureServiceLayerOptionsPartial(IServiceCollection services)
        {
            services.Configure<DbInitializerOptions>(Configuration.GetSection(DbInitializerOptions.DefaultSectionName));

            services.Configure<ApplicationOptions>(Configuration.GetSection(ApplicationOptions.DefaultSectionName));

            services.Configure<PasswordOptions>(Configuration.GetSection("Security:Passwords"));
            services.Configure<LockoutOptions>(Configuration.GetSection("Security:Lockout"));

            services.Configure<SmtpOptions>(Configuration.GetSection(SmtpOptions.DefaultSectionName));

            services.Configure<MailingOptions>(Configuration.GetSection(MailingOptions.DefaultSectionName));
        }
    }
}
