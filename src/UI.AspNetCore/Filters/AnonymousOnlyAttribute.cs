using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApp.UI.Areas.Dashboard;

namespace WebApp.UI.Filters
{
    public class AnonymousOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                filterContext.Result = new RedirectToRouteResult(DashboardRoutes.OverviewRouteName, new { id = (object?)null });
        }
    }
}
