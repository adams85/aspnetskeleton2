using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WebApp.Common.Roles;
using WebApp.Service;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Settings;
using WebApp.UI.Areas.Dashboard.Models;
using WebApp.UI.Areas.Dashboard.Models.Settings;
using WebApp.UI.Helpers;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Controllers
{
    [Authorize(Roles = nameof(RoleEnum.Administators))]
    [Area(DashboardRoutes.AreaName)]
    [Route("[area]/[controller]/[action]")]
    public class SettingsController : Controller
    {
        private const string SettingEditorTemplateName = "EditorTemplates/" + nameof(SettingEditModel);

        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        public SettingsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, IStringLocalizer<SettingsController>? stringLocalizer)
        {
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            T = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
        }

        public IStringLocalizer T { get; }

        [HttpGet("/[area]/[controller]", Name = DashboardRoutes.SettingsRouteName)]
        public IActionResult Index(ListSettingsQuery query)
        {
            var model = new DashboardPageModel<ListSettingsQuery>
            {
                Content = query
            };

            return View(model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(string id, [FromQuery] string? returnUrl)
        {
            var item = await _queryDispatcher.DispatchAsync(new GetSettingQuery
            {
                Name = id,
                IncludeDescription = true
            }, HttpContext.RequestAborted);

            if (item == null)
                return NotFound();

            var model = new SettingEditModel
            {
                Item = item,
                IsNewItem = false,
                EditorTemplateName = RouteData.Values.GetDefaultViewPath(SettingEditorTemplateName),
                ReturnUrl = EnsureReturnUrl(returnUrl)
            };

            return View(Pages.EditPopupViewName, model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("{id}")]
        public async Task<IActionResult> Edit(string id, SettingEditModel model, [FromQuery] string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var command = model.Item.ToUpdateCommand();

                try
                {
                    await _commandDispatcher.DispatchAsync(command, HttpContext.RequestAborted);
                }
                catch (ServiceErrorException ex) when (ex.ErrorCode == ServiceErrorCode.ParamNotValid && (string)ex.Args[0] == nameof(SettingData.Value))
                {
                    ModelState.AddModelError(nameof(SettingEditModel.Item) + "." + nameof(SettingData.Value), T["Invalid value."].Value);
                }

                if (ModelState.IsValid)
                    return Redirect(EnsureReturnUrl(returnUrl));
            }

            var item = await _queryDispatcher.DispatchAsync(new GetSettingQuery
            {
                Name = id,
                IncludeDescription = true
            }, HttpContext.RequestAborted);

            if (item == null)
                return NotFound();

            model.Item.DefaultValue = item.DefaultValue;
            model.Item.MinValue = item.MinValue;
            model.Item.MaxValue = item.MaxValue;
            model.Item.Description = item.Description;

            model.IsNewItem = false;
            model.EditorTemplateName = RouteData.Values.GetDefaultViewPath(SettingEditorTemplateName);
            model.ReturnUrl = EnsureReturnUrl(returnUrl);

            return View(Pages.EditPopupViewName, model);
        }

        [NonAction]
        private string EnsureReturnUrl(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) ? returnUrl : Url.RouteUrl(DashboardRoutes.SettingsRouteName);
    }
}
