using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;

namespace WebApp.UI.Infrastructure.ViewFeatures
{
    public sealed class GlobalRazorHelpersFactory : IGlobalRazorHelpersFactory
    {
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IRazorPageActivator _razorPageActivator;

        private readonly ConcurrentDictionary<Type, string> _helpersTypeViewPathMappings;

        public GlobalRazorHelpersFactory(ICompositeViewEngine viewEngine, IRazorPageActivator razorPageActivator, IOptions<GlobalRazorHelpersOptions>? options)
        {
            _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            _razorPageActivator = razorPageActivator ?? throw new ArgumentNullException(nameof(razorPageActivator));

            var optionsValue = options?.Value;
            _helpersTypeViewPathMappings = new ConcurrentDictionary<Type, string>(optionsValue?.HelpersTypeViewPathMappings ?? Enumerable.Empty<KeyValuePair<Type, string>>());
        }

        public IRazorPage CreateRazorPage(string helpersViewPath, ViewContext viewContext)
        {
            var viewEngineResult = _viewEngine.GetView(viewContext.ExecutingFilePath, helpersViewPath, isMainPage: false);

            var originalLocations = viewEngineResult.SearchedLocations;

            if (!viewEngineResult.Success)
                viewEngineResult = _viewEngine.FindView(viewContext, helpersViewPath, isMainPage: false);

            if (!viewEngineResult.Success)
            {
                var locations = string.Empty;

                if (originalLocations.Any())
                    locations = Environment.NewLine + string.Join(Environment.NewLine, originalLocations);

                if (viewEngineResult.SearchedLocations.Any())
                    locations += Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations);

                throw new InvalidOperationException($"The Razor helpers view '{helpersViewPath}' was not found. The following locations were searched:{locations}");
            }

            var razorPage = ((RazorView)viewEngineResult.View).RazorPage;

            razorPage.ViewContext = viewContext;

            // we need to save and restore the original view data dictionary as it is changed by IRazorPageActivator.Activate
            // https://github.com/dotnet/aspnetcore/blob/v3.1.18/src/Mvc/Mvc.Razor/src/RazorPagePropertyActivator.cs#L59
            var originalViewData = viewContext.ViewData;
            try { _razorPageActivator.Activate(razorPage, viewContext); }
            finally { viewContext.ViewData = originalViewData; }

            return razorPage;
        }

        public dynamic Create(string helpersViewPath, ViewContext viewContext) => CreateRazorPage(helpersViewPath, viewContext);

        public THelpers Create<THelpers>(ViewContext viewContext) where THelpers : class
        {
            var helpersViewPath = _helpersTypeViewPathMappings.GetOrAdd(typeof(THelpers), type => "Helpers/_" + (type.Name.StartsWith("I") ? type.Name.Substring(1) : type.Name));

            return (THelpers)CreateRazorPage(helpersViewPath, viewContext);
        }
    }
}
