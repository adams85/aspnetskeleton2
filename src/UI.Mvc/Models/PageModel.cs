using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models
{
    public class PageModel : ILayoutModelProvider<PageLayoutModel>
    {
        private PageLayoutModel? _layout;
        public PageLayoutModel Layout
        {
            get => _layout ??= new PageLayoutModel();
            set => _layout = value;
        }
    }

    public class PageModel<TContent> : PageModel
    {
        public TContent Content { get; set; } = default!;
    }
}
