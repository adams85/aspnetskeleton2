using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure.Templating
{
    public interface ITemplateRenderer
    {
        Task<string> RenderAsync<TModel>(string templateName, TModel model, CultureInfo? culture = null, CultureInfo? uiCulture = null, CancellationToken cancellationToken = default);
    }
}
