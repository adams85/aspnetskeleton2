using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.UI.Infrastructure.ViewFeatures
{
    public sealed class GlobalRazorHelpers<THelpers> : IGlobalRazorHelpers<THelpers>
        where THelpers : class
    {
        private readonly IGlobalRazorHelpersFactory _razorHelpersFactory;

        public GlobalRazorHelpers(IGlobalRazorHelpersFactory razorHelpersFactory)
        {
            _razorHelpersFactory = razorHelpersFactory ?? throw new ArgumentNullException(nameof(razorHelpersFactory));
        }

        private THelpers? _instance;
        public THelpers Instance => _instance ?? throw new InvalidOperationException("The service was not contextualized.");

        public void Contextualize(ViewContext viewContext) => _instance = _razorHelpersFactory.Create<THelpers>(viewContext);
    }
}
