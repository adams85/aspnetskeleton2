using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models
{
    public class CardPageModel : ILayoutModelProvider<CardPageLayoutModel>
    {
        private CardPageLayoutModel? _layout;
        public CardPageLayoutModel Layout
        {
            get => _layout ??= new CardPageLayoutModel();
            set => _layout = value;
        }
    }

    public class CardPageModel<TContent> : CardPageModel
    {
        public TContent Content { get; set; } = default!;
    }
}
