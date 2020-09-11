using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models
{
    public class PageModel : ILayoutModelProvider<PageLayoutModel>
    {
        private PageLayoutModel? _layout;
        [BindNever]
        public PageLayoutModel Layout
        {
            get => _layout ??= new PageLayoutModel();
            set => _layout = value;
        }
    }
}
