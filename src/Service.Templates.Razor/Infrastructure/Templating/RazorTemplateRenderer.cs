using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
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
using WebApp.Core.Helpers;

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

        private bool TryGetTemplateView(string templateName, out IEnumerable<string> searchedLocations, [MaybeNullWhen(false)] out IView templateView)
        {
            templateName = TemplatesBasePath + "/" + templateName + RazorViewEngine.ViewExtension;

            var getViewResult = _viewEngine.GetView(null, templateName, isMainPage: true);

            searchedLocations = getViewResult.SearchedLocations ?? Enumerable.Empty<string>();

            if (!getViewResult.Success)
            {
                templateView = default;
                return false;
            }

            templateView = getViewResult.View;
            return true;
        }

        private IView FindTemplateView(string templateName)
        {
            if (!TryGetTemplateView(templateName, out var searchedLocations, out var templateView))
            {
                var errorMessage = string.Join(Environment.NewLine, searchedLocations.Prepend($"Template '{templateName}' was not found at any of the following locations:"));
                throw new InvalidOperationException(errorMessage);
            }

            return templateView;
        }

        public async Task<string> RenderAsync<TModel>(string templateName, TModel model, CultureInfo? culture = null, CultureInfo? uiCulture = null, CancellationToken cancellationToken = default)
        {
            await using (var scope = DisposableAdapter.From(_serviceScopeFactory.CreateScope()))
            {
                culture ??= CultureInfo.CurrentCulture;
                uiCulture ??= CultureInfo.CurrentUICulture;

                var templateView = FindTemplateView(templateName);

                var httpContext = new DefaultHttpContext { RequestServices = scope.Value.ServiceProvider };
                var actionContext = new ActionContext(httpContext, s_routeData, s_actionDescriptor);

                using (var writer = new StringWriter())
                {
                    var viewData = new ViewDataDictionary<TModel>(s_modelMetadataProvider, new ModelStateDictionary())
                    {
                        Model = model
                    };

                    var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

                    var viewContext = new ViewContext(actionContext, templateView, viewData, tempData, writer, s_htmlHelperOptions);

                    (culture, CultureInfo.CurrentCulture) = (CultureInfo.CurrentCulture, culture);
                    try
                    {
                        (uiCulture, CultureInfo.CurrentUICulture) = (CultureInfo.CurrentUICulture, uiCulture);
                        try
                        {
                            await templateView.RenderAsync(viewContext).AsCancelable(cancellationToken).ConfigureAwait(false);
                        }
                        finally { CultureInfo.CurrentUICulture = uiCulture; }
                    }
                    finally { CultureInfo.CurrentCulture = culture; }

                    return writer.ToString();
                }
            }
        }
    }
}
