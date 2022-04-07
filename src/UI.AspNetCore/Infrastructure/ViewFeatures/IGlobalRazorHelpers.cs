using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WebApp.UI.Infrastructure.ViewFeatures;

public interface IGlobalRazorHelpers<out THelpers> : IViewContextAware
    where THelpers : class
{
    THelpers Instance { get; }
}
