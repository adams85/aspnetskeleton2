namespace WebApp.UI.Models.Layout;

public interface ILayoutModelProvider<out TLayoutModel> where TLayoutModel : LayoutModel
{
    TLayoutModel Layout { get; }
}
