using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models
{
    public class EditModel : ILayoutModelProvider<PageLayoutModel>
    {
        public EditModel()
        {
            GetItemDisplayName = GetDefaultItemDisplayName;
            GetTitle = GetDefaultTitle;
        }

        private PageLayoutModel? _layout;
        public PageLayoutModel Layout
        {
            get => _layout ??= new PageLayoutModel
            {
                GetTitle = (httpContext, htmlLocalizer) => GetTitle(httpContext, htmlLocalizer)
            };
            set => _layout = value;
        }

        [BindNever, ValidateNever]
        public Func<HttpContext, IHtmlLocalizer, string> GetItemDisplayName { get; set; }

        protected virtual string GetDefaultItemDisplayName(HttpContext httpContext, IHtmlLocalizer htmlLocalizer)
        {
#pragma warning disable IDE1006 // Naming Styles
            var T = htmlLocalizer;
#pragma warning restore IDE1006 // Naming Styles

            return T["Item"].Value;
        }

        [BindNever, ValidateNever]
        public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle { get; set; }

        protected virtual LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer htmlLocalizer)
        {
#pragma warning disable IDE1006 // Naming Styles
            var T = htmlLocalizer;
#pragma warning restore IDE1006 // Naming Styles

            return IsNewItem ? T["Create {0}", GetItemDisplayName(httpContext, T)] : T["Edit {0}", GetItemDisplayName(httpContext, T)];
        }

        [BindNever]
        public string? EditorTemplateName { get; set; }

        [BindNever]
        public bool IsNewItem { get; set; }

        [BindNever]
        public string? ReturnUrl { get; set; }
    }

    public class EditModel<TItem> : EditModel
    {
        public TItem Item { get; set; } = default!;

        protected override string GetDefaultItemDisplayName(HttpContext httpContext, IHtmlLocalizer htmlLocalizer)
        {
            var modelMetadataProvider = httpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
            var itemDisplayName = modelMetadataProvider.GetMetadataForType(typeof(TItem))?.DisplayName;
            return itemDisplayName ?? base.GetDefaultItemDisplayName(httpContext, htmlLocalizer);
        }
    }
}
