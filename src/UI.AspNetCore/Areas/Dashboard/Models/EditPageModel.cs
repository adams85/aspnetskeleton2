using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models;

public abstract class EditPageModel<TPageDescriptor> : CardPageModel<TPageDescriptor>, IEditPopupModel
    where TPageDescriptor : EditPageDescriptor, new()
{
    public virtual Func<HttpContext, IHtmlLocalizer, string> GetItemDisplayName => PageDescriptor.GetItemDisplayName;
    public virtual Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle => PageDescriptor.GetDefaultTitle;
    public abstract string? EditorTemplateName { get; }
    public bool IsNewItem => PageDescriptor.CreatesItem;

    private string? _returnUrl;
    [AllowNull]
    public string ReturnUrl
    {
        get => EnsureReturnUrl(_returnUrl);
        protected set => _returnUrl = value;
    }

    protected abstract string DefaultReturnUrl { get; }

    protected string EnsureReturnUrl(string? returnUrl) => !string.IsNullOrEmpty(returnUrl) ? returnUrl : DefaultReturnUrl;
}

public abstract class EditPageModel<TPageDescriptor, TItem> : EditPageModel<TPageDescriptor>, IEditPopupModel<TItem>
    where TPageDescriptor : EditPageDescriptor<TItem>, new()
{
    [BindProperty] public TItem Item { get; set; } = default!;
}
