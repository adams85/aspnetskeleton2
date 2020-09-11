namespace WebApp.UI.Models
{
    public sealed class SingleValuePageModel<TValue> : PageModel
    {
        public TValue Value { get; set; } = default!;
    }
}
