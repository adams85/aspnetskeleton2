using Microsoft.AspNetCore.Http;

namespace WebApp.UI.Models.Layout
{
    public class CardPageLayoutModel : LayoutModel
    {
        protected override string? GetActualLayoutName(HttpContext httpContext) =>
            base.GetActualLayoutName(httpContext) ?? "_CardPageLayout";
    }
}
