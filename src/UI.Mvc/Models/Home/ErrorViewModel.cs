using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models.Home
{
    public class ErrorViewModel : PopupPageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
