using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models
{
    public abstract class DeletePageModel<TPageDescriptor> : CardPageModel<TPageDescriptor>, IDeletePopupModel
        where TPageDescriptor : DeletePageDescriptor, new()
    {
        public virtual Func<HttpContext, IHtmlLocalizer, string> GetItemDisplayName => PageDescriptor.GetItemDisplayName;
        public virtual Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle => PageDescriptor.GetDefaultTitle;
        public string ItemId { get; set; } = null!;

        private string? _returnUrl;
        [AllowNull]
        public string ReturnUrl
        {
            get => EnsureReturnUrl(_returnUrl);
            set => _returnUrl = value;
        }

        protected abstract string DefaultReturnUrl { get; }

        protected string EnsureReturnUrl(string? returnUrl) => !string.IsNullOrEmpty(returnUrl) ? returnUrl : DefaultReturnUrl;
    }

    public abstract class DeletePageModel<TPageDescriptor, TItem> : DeletePageModel<TPageDescriptor>
        where TPageDescriptor : DeletePageDescriptor<TItem>, new()
    {
    }
}
