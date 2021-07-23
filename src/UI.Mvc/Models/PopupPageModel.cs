using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models
{
    public class PopupPageModel : ILayoutModelProvider<PopupPageLayoutModel>
    {
        private PopupPageLayoutModel? _layout;
        public PopupPageLayoutModel Layout
        {
            get => _layout ??= new PopupPageLayoutModel();
            set => _layout = value;
        }
    }

    public class PopupPageModel<TContent> : PopupPageModel
    {
        public TContent Content { get; set; } = default!;
    }
}
