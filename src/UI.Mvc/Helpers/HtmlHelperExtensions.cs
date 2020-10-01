using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.UI.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent JavaScriptString(this IHtmlHelper htmlHelper, string value, bool addQuotes = true) =>
            htmlHelper.Raw(HttpUtility.JavaScriptStringEncode(value, addQuotes));
    }
}
