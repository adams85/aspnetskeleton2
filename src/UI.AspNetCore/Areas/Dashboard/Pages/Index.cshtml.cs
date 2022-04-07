using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.Core.Infrastructure;
using WebApp.UI.Areas.Dashboard.Models;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Pages;

[Authorize]
public class IndexModel : DashboardPageModel<IndexModel.PageDescriptorClass>
{
    private readonly IClock _clock;

    public IndexModel(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public DateTime UtcNow { get; private set; }
    public TimeSpan TimeToMidnight { get; private set; }

    public void OnGet()
    {
        var utcNow = _clock.UtcNow;
        var midnight = utcNow.Date + TimeSpan.FromDays(1);

        UtcNow = utcNow;
        TimeToMidnight = midnight - utcNow;
    }

    public sealed class PageDescriptorClass : PageDescriptor
    {
        public override string PageName => "/Index";
        public override string AreaName => DashboardConstants.AreaName;

        public override LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer t) => t["Overview"];
    }
}
