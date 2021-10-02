using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;

namespace WebApp.UI.Areas.Dashboard.Models
{
    public interface IDeletePopupModel
    {       
        Func<HttpContext, IHtmlLocalizer, string> GetItemDisplayName { get; }
        Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle { get; }
        string ItemId { get; }
        string ReturnUrl { get; }
    }
}
