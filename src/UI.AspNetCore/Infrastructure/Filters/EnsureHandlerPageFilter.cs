using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.UI.Infrastructure.Filters
{
    public sealed class EnsureHandlerPageFilter : IPageFilter, IOrderedFilter
    {
        // let HandleOptionsRequestsPageFilter run first
        // https://github.com/dotnet/aspnetcore/blob/v3.1.18/src/Mvc/Mvc.RazorPages/src/Infrastructure/HandleOptionsRequestsPageFilter.cs#L30
        public int Order => 2000;

        public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (context.HandlerMethod == null && context.Result == null)
                context.Result = new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
    }
}
