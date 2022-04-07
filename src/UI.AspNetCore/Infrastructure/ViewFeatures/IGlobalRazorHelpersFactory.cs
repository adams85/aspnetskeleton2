using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.UI.Infrastructure.ViewFeatures;

public interface IGlobalRazorHelpersFactory
{
    dynamic Create(string helpersViewPath, ViewContext viewContext);
    THelpers Create<THelpers>(ViewContext viewContext) where THelpers : class;
}
