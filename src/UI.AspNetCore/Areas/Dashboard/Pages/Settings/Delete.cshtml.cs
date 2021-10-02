using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Common.Roles;
using WebApp.Service.Infrastructure;
using WebApp.Service.Settings;
using WebApp.UI.Areas.Dashboard.Models;
using WebApp.UI.Helpers;

namespace WebApp.UI.Areas.Dashboard.Pages.Settings
{
    // this page is created for the sake of completeness (settings cannot be deleted)
    [Authorize(Roles = nameof(RoleEnum.Administators))]
    public class DeleteModel : DeletePageModel<DeleteModel.PageDescriptorClass, SettingData>
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        public DeleteModel(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        }

        protected override string DefaultReturnUrl => Url.Page(IndexModel.PageDescriptor.PageName, new { area = IndexModel.PageDescriptor.AreaName });

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] string? returnUrl)
        {
            var item = await _queryDispatcher.DispatchAsync(new GetSettingQuery
            {
                Name = id,
                IncludeDescription = true
            }, HttpContext.RequestAborted);

            if (item == null)
                return NotFound();

            ItemId = item.Name;
            ReturnUrl = returnUrl;

            return HttpContext.Request.IsAjaxRequest() ? Partial(DeletePageDescriptor.DeletePopupPartialViewName, this) : (IActionResult)Page();
        }

        public Task<IActionResult> OnPost([FromRoute] string id, [FromQuery] string? returnUrl)
        {
            throw new NotImplementedException();
        }

        public sealed class PageDescriptorClass : DeletePageDescriptor<SettingData>
        {
            public override string PageName => "/Settings/Delete";
            public override string AreaName => DashboardConstants.AreaName;
        }
    }
}
