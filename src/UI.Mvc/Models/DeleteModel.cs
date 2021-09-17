using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models
{
    public class DeleteModel : ILayoutModelProvider<CardPageLayoutModel>
    {
        public DeleteModel()
        {
            GetItemDisplayName = GetDefaultItemDisplayName;
            GetTitle = GetDefaultTitle;
        }

        private CardPageLayoutModel? _layout;
        public CardPageLayoutModel Layout
        {
            get => _layout ??= new CardPageLayoutModel
            {
                GetTitle = (httpContext, htmlLocalizer) => GetTitle(httpContext, htmlLocalizer)
            };
            set => _layout = value;
        }

        [BindNever, ValidateNever]
        public Func<HttpContext, IHtmlLocalizer, string> GetItemDisplayName { get; set; }

        protected virtual string GetDefaultItemDisplayName(HttpContext httpContext, IHtmlLocalizer htmlLocalizer)
        {
            var t = htmlLocalizer;
            return t["Item"].Value;
        }

        [BindNever, ValidateNever]
        public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle { get; set; }

        protected virtual LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer htmlLocalizer)
        {
            var t = htmlLocalizer;
            return t["Delete {0}", GetItemDisplayName(httpContext, t)];
        }

        public string ItemId { get; set; } = null!;

        [BindNever]
        public string? ReturnUrl { get; set; }
    }

    public class DeleteModel<TItem> : DeleteModel
    {
        protected override string GetDefaultItemDisplayName(HttpContext httpContext, IHtmlLocalizer htmlLocalizer)
        {
            var modelMetadataProvider = httpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
            var itemDisplayName = modelMetadataProvider.GetMetadataForType(typeof(TItem))?.DisplayName;
            return itemDisplayName ?? base.GetDefaultItemDisplayName(httpContext, htmlLocalizer);
        }
    }
}
