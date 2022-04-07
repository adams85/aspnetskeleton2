using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WebApp.Common.Roles;
using WebApp.Service;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Settings;
using WebApp.UI.Areas.Dashboard.Models;
using WebApp.UI.Helpers;

namespace WebApp.UI.Areas.Dashboard.Pages.Settings
{
    [Authorize(Roles = nameof(RoleEnum.Administrators))]
    public class EditModel : EditPageModel<EditModel.PageDescriptorClass, SettingData>
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IStringLocalizer _t;

        public EditModel(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, IStringLocalizer<EditModel>? stringLocalizer)
        {
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _t = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
        }

        public override string? EditorTemplateName => UIUrlHelper.GetRelativePath(PageContext.ActionDescriptor.RelativePath, "EditorTemplates/SettingEditModel.cshtml");

        protected override string DefaultReturnUrl => Url.Page(IndexModel.PageDescriptor.PageName, new { area = IndexModel.PageDescriptor.AreaName })!;

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] string? returnUrl)
        {
            var item = await _queryDispatcher.DispatchAsync(new GetSettingQuery
            {
                Name = id,
                IncludeDescription = true
            }, HttpContext.RequestAborted);

            if (item == null)
                return NotFound();

            Item = item;
            ReturnUrl = returnUrl;

            return HttpContext.Request.IsAjaxRequest() ? Partial(EditPageDescriptor.EditPopupPartialViewName, this) : (IActionResult)Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string id, [FromQuery] string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var command = Item.ToUpdateCommand();

                try
                {
                    await _commandDispatcher.DispatchAsync(command, HttpContext.RequestAborted);
                }
                catch (ServiceErrorException ex) when (ex.ErrorCode == ServiceErrorCode.ParamNotValid && (string)ex.Args[0] == nameof(SettingData.Value))
                {
                    ModelState.AddModelError(nameof(Item) + "." + nameof(SettingData.Value), _t["Invalid value."].Value);
                }

                if (ModelState.IsValid)
                    return HttpContext.Request.IsAjaxRequest() ? NoContent() : (IActionResult)Redirect(EnsureReturnUrl(returnUrl));
            }

            var item = await _queryDispatcher.DispatchAsync(new GetSettingQuery
            {
                Name = id,
                IncludeDescription = true
            }, HttpContext.RequestAborted);

            if (item == null)
                return NotFound();

            Item = Item with
            {
                DefaultValue = item.DefaultValue,
                MinValue = item.MinValue,
                MaxValue = item.MaxValue,
                Description = item.Description,
            };

            ReturnUrl = returnUrl;

            return HttpContext.Request.IsAjaxRequest() ? Partial(EditPageDescriptor.EditPopupPartialViewName, this) : (IActionResult)Page();
        }

        public sealed class PageDescriptorClass : EditPageDescriptor<SettingData>
        {
            public override string PageName => "/Settings/Edit";
            public override string AreaName => DashboardConstants.AreaName;
            public override bool CreatesItem => false;
        }
    }
}
