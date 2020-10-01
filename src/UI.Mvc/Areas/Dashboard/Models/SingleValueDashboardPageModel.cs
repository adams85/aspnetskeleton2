namespace WebApp.UI.Areas.Dashboard.Models
{
    public sealed class SingleValueDashboardPageModel<TValue> : DashboardPageModel
    {
        public TValue Value { get; set; } = default!;
    }
}
