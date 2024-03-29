﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.Common.Roles;
using WebApp.Service;
using WebApp.Service.Infrastructure;
using WebApp.Service.Settings;
using WebApp.UI.Areas.Dashboard.Models;
using WebApp.UI.Areas.Dashboard.Models.DataTables;
using WebApp.UI.Helpers;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Pages.Settings;

[Authorize(Roles = nameof(RoleEnum.Administrators))]
public class IndexModel : ListPageModel<IndexModel.PageDescriptorClass, ListSettingsQuery, ListResult<SettingData>, SettingData>
{
    public static LocalizedHtmlString GetDescription(SettingData data)
    {
        var description = data.Description ?? string.Empty;
        return new LocalizedHtmlString(description, description, isResourceNotFound: false, data.DefaultValue!, data.MinValue!, data.MaxValue!);
    }

    private readonly IQueryDispatcher _queryDispatcher;

    public IndexModel(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
    }

    public string DisplayPartialName => UIUrlHelper.GetRelativePath(PageContext.ActionDescriptor.RelativePath, "Partials/_SettingsTable.cshtml");

    public async Task<IActionResult> OnGet([FromQuery] ListSettingsQuery query)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        Query = query;
        Result = await _queryDispatcher.DispatchAsync(query, HttpContext.RequestAborted);

        ViewData[nameof(IndexModel)] = this;

        return
            HttpContext.Request.IsAjaxRequest() ?
            PartialWithCurrentViewData(DisplayPartialName, query) :
            Page();
    }

    public static class ColumnBindings
    {
        public static readonly DataTableColumnBinding Name = DataTableColumnBinding.For<SettingData>(m => m.Name);
        public static readonly DataTableColumnBinding Value = DataTableColumnBinding.For<SettingData>(m => m.Value);
        public static readonly DataTableColumnBinding Description = DataTableColumnBinding.For<SettingData>(m => m.Description);
    }

    public sealed class PageDescriptorClass : PageDescriptor
    {
        public override string PageName => "/Settings/Index";
        public override string AreaName => DashboardConstants.AreaName;

        public override LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer t) => t["Application Settings"];
    }
}
