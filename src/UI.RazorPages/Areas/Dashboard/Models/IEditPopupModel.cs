using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;

namespace WebApp.UI.Areas.Dashboard.Models;

public interface IEditPopupModel
{
    Func<HttpContext, IHtmlLocalizer, string> GetItemDisplayName { get; }
    Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle { get; }
    string? EditorTemplateName { get; }
    bool IsNewItem { get; }
    string ReturnUrl { get; }
}

public interface IEditPopupModel<TItem> : IEditPopupModel
{
    TItem Item { get; }
}
