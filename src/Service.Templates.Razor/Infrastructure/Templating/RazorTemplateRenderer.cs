using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Service.Infrastructure.Templating
{
    internal sealed class RazorTemplateRenderer : ITemplateRenderer
    {
        private const string TemplatesBasePath = "/Templates";

        private static readonly RouteData s_routeData = new RouteData();
        private static readonly ActionDescriptor s_actionDescriptor = new ActionDescriptor();
        private static readonly EmptyModelMetadataProvider s_modelMetadataProvider = new EmptyModelMetadataProvider();
        private static readonly HtmlHelperOptions s_htmlHelperOptions = new HtmlHelperOptions();

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;

        public RazorTemplateRenderer(IServiceScopeFactory serviceScopeFactory, IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            _tempDataProvider = tempDataProvider ?? throw new ArgumentNullException(nameof(tempDataProvider));
        }

        public async Task<string> RenderAsync<TModel>(string templateName, TModel model)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var httpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
                var actionContext = new ActionContext(httpContext, s_routeData, s_actionDescriptor);

                var templateView = GetTemplateView(templateName);

                using (var writer = new StringWriter())
                {
                    var viewData = new ViewDataDictionary<TModel>(s_modelMetadataProvider, new ModelStateDictionary())
                    {
                        Model = model
                    };

                    var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

                    var viewContext = new ViewContext(actionContext, templateView, viewData, tempData, writer, s_htmlHelperOptions);

                    await templateView.RenderAsync(viewContext).ConfigureAwait(false);

                    return writer.ToString();
                }
            }
        }

        private IView GetTemplateView(string templateName)
        {
            templateName = TemplatesBasePath + "/" + templateName + RazorViewEngine.ViewExtension;

            var getViewResult = _viewEngine.GetView(null, templateName, isMainPage: true);
            if (!getViewResult.Success)
            {
                var errorLines = new[] { $"Template '{templateName}' was not found at any of the following locations:" }
                    .Concat(getViewResult.SearchedLocations);

                var errorMessage = string.Join(Environment.NewLine, errorLines);

                throw new InvalidOperationException(errorMessage);
            }

            return getViewResult.View;
        }
    }
}
