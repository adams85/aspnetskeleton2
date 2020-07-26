using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.UI.Filters
{
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                filterContext.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}