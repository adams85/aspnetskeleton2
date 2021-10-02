using System;
using System.Collections.Generic;
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

namespace WebApp.UI.Areas.Dashboard.Pages.Settings
{
    [Authorize(Roles = nameof(RoleEnum.Administators))]
    public class IndexModel : ListPageModel<IndexModel.PageDescriptorClass, ListSettingsQuery, ListResult<SettingData>, SettingData>
    {
        public static LocalizedHtmlString GetDescription(SettingData data) =>
            new LocalizedHtmlString(data.Description, data.Description, false, data.DefaultValue, data.MinValue, data.MaxValue);

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
                (IActionResult)Page();
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
            public override Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; } = (_, t) => t["Application Settings"];
        }
    }
}
