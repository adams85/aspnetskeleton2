﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApp.UI.Areas.Dashboard.Controllers;
using WebApp.UI.Helpers;

namespace WebApp.UI.Filters
{
    public class AnonymousOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                filterContext.Result = new RedirectToActionResult(
                    nameof(HomeController.Index),
                    MvcHelper.GetControllerName<HomeController>(),
                    new { area = UIConstants.DashboardAreaName, id = (object?)null });
        }
    }
}
