using WebApp.UI.Areas.Dashboard.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.UI.Filters
{
    public class AnonymousOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                filterContext.Result = new RedirectToActionResult(nameof(HomeController.Index), "Home", new { area = "Dashboard", id = (object?)null });
        }
    }
}
