using Microsoft.AspNetCore.Http;

namespace WebApp.UI.Models.Layout
{
    public class PopupPageLayoutModel : LayoutModel
    {
        protected override string? GetActualLayoutName(HttpContext httpContext) =>
            base.GetActualLayoutName(httpContext) ?? "_PopupPageLayout";
    }
}
