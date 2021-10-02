using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models
{
    public abstract class CardPageModel<TPageDescriptor> : BasePageModel<TPageDescriptor>, ILayoutModelProvider<CardPageLayoutModel>
        where TPageDescriptor : PageDescriptor, new()
    {
        private CardPageLayoutModel? _layout;
        public CardPageLayoutModel Layout
        {
            get => _layout ??= new CardPageLayoutModel();
            set => _layout = value;
        }
    }
}
