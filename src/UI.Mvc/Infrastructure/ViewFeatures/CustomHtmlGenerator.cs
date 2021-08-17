using System.Collections.Generic;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebApp.UI.Infrastructure.ViewFeatures
{
    public sealed class CustomHtmlGenerator : DefaultHtmlGenerator
    {
        private static IHtmlHelper GetHtmlHelperFor(ViewContext viewContext)
        {
            const string htmlHelperViewDataKey = nameof(CustomHtmlGenerator) + "_" + nameof(IHtmlHelper);

            if (!viewContext.ViewData.TryGetValue(htmlHelperViewDataKey, out var htmlHelperObj) || !(htmlHelperObj is IHtmlHelper htmlHelper))
                viewContext.ViewData[htmlHelperViewDataKey] = htmlHelper = GetViewHtmlHelper(viewContext) ?? CreateHtmlHelper(viewContext);

            return htmlHelper;

            static IHtmlHelper? GetViewHtmlHelper(ViewContext viewContext)
            {
                if (!(viewContext.View is RazorView razorView))
                    return null;

                dynamic razorPage = razorView.RazorPage;

                try { return (IHtmlHelper)razorPage.Html; }
                catch { return null; }
            }

            static IHtmlHelper CreateHtmlHelper(ViewContext viewContext)
            {
                var htmlHelper = viewContext.HttpContext.RequestServices.GetRequiredService<IHtmlHelper>();
                (htmlHelper as IViewContextAware)?.Contextualize(viewContext);
                return htmlHelper;
            }
        }

        private static TagBuilder AddBootstrapValidationCssClasses(ViewContext viewContext, string expression, TagBuilder tagBuilder)
        {
            // we need to get the model property key from the expression, which functionality is buried in an internal class unfortunately
            // (https://github.com/dotnet/aspnetcore/blob/v3.1.18/src/Mvc/Mvc.ViewFeatures/src/NameAndIdProvider.cs#L147)
            // however, this internal API is exposed via the IHtmlHelper.Name method:
            // (https://github.com/dotnet/aspnetcore/blob/v3.1.18/src/Mvc/Mvc.ViewFeatures/src/HtmlHelper.cs#L451)
            var htmlHelper = GetHtmlHelperFor(viewContext);
            var fullName = htmlHelper.Name(expression);

            if (viewContext.ModelState.TryGetValue(fullName, out var entry))
            {
                if (entry.ValidationState == ModelValidationState.Invalid)
                    tagBuilder.AddCssClass("is-invalid");
                else if (entry.ValidationState == ModelValidationState.Valid)
                    tagBuilder.AddCssClass("is-valid");
            }

            return tagBuilder;
        }

        public CustomHtmlGenerator(IAntiforgery antiforgery, IOptions<MvcViewOptions> optionsAccessor, IModelMetadataProvider metadataProvider, IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder, ValidationHtmlAttributeProvider validationAttributeProvider)
            : base(antiforgery, optionsAccessor, metadataProvider, urlHelperFactory, htmlEncoder, validationAttributeProvider) { }

        protected override TagBuilder GenerateInput(ViewContext viewContext, InputType inputType, ModelExplorer modelExplorer, string expression, object value, bool useViewData, bool isChecked, bool setId, bool isExplicitValue, string format, IDictionary<string, object> htmlAttributes) =>
            AddBootstrapValidationCssClasses(viewContext, expression, base.GenerateInput(viewContext, inputType, modelExplorer, expression, value, useViewData, isChecked, setId, isExplicitValue, format, htmlAttributes));

        public override TagBuilder GenerateSelect(ViewContext viewContext, ModelExplorer modelExplorer, string optionLabel, string expression, IEnumerable<SelectListItem> selectList, ICollection<string> currentValues, bool allowMultiple, object htmlAttributes) =>
            AddBootstrapValidationCssClasses(viewContext, expression, base.GenerateSelect(viewContext, modelExplorer, optionLabel, expression, selectList, currentValues, allowMultiple, htmlAttributes));

        public override TagBuilder GenerateTextArea(ViewContext viewContext, ModelExplorer modelExplorer, string expression, int rows, int columns, object htmlAttributes) =>
            AddBootstrapValidationCssClasses(viewContext, expression, base.GenerateTextArea(viewContext, modelExplorer, expression, rows, columns, htmlAttributes));
    }
}
