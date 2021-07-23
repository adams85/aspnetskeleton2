using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApp.Service.Infrastructure;
using WebApp.Service.Settings;
using WebApp.UI.Areas.Dashboard.Models.Settings;

namespace WebApp.UI.Areas.Dashboard.ViewComponents.Layout
{
    public class SettingsTableViewComponent : ViewComponent
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ISettingsProvider _settingsProvider;

        public SettingsTableViewComponent(IQueryDispatcher queryDispatcher, ISettingsProvider settingsProvider)
        {
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        }

        public async Task<IViewComponentResult> InvokeAsync(ListSettingsQuery query, string? prefix = null)
        {
            query.EnsurePaging(ListSettingsQuery.DefaultPageSize, _settingsProvider.MaxPageSize());

            ViewData[nameof(SettingListModel)] = new SettingListModel
            {
                Query = query,
                Result = await _queryDispatcher.DispatchAsync(query, HttpContext.RequestAborted)
            };

            ViewData.TemplateInfo.HtmlFieldPrefix = prefix;

            return View(query);
        }
    }
}
