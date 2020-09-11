using System;

namespace WebApp.UI.Areas.Dashboard.Models.Home
{
    public class IndexModel : DashboardPageModel
    {
        public DateTime UtcNow { get; set; }
        public TimeSpan TimeToMidnight { get; set; }
    }
}
