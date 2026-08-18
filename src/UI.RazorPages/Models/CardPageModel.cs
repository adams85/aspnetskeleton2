using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models;

public abstract class CardPageModel<TPageDescriptor> : BasePageModel<TPageDescriptor>, ILayoutModelProvider<CardPageLayoutModel>
    where TPageDescriptor : PageDescriptor, new()
{
    public CardPageLayoutModel Layout
    {
        get => field ??= new CardPageLayoutModel();
        set;
    }
}
