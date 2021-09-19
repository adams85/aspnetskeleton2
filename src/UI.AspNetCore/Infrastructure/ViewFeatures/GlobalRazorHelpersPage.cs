using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;

namespace WebApp.UI.Infrastructure.ViewFeatures
{
    public class GlobalRazorHelpersPage : RazorPageBase
    {
        protected IHtmlContent HelperToHtmlContent<TState>(Action<TextWriter, TState> helper, TState state) => new HelperResult(writer =>
        {
            PushWriter(writer);
            helper(writer, state);
            PopWriter();
            return Task.CompletedTask;
        });

        public override void BeginContext(int position, int length, bool isLiteral) { }
        public override void EndContext() { }
        public override void EnsureRenderedBodyOrSections() { }
        public override Task ExecuteAsync() => Task.CompletedTask;
    }
}
