using WebApp.UI.Models.Layout;

namespace WebApp.UI.Models.Home
{
    public class ErrorViewModel : CardPageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
