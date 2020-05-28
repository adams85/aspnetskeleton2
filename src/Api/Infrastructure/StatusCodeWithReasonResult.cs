using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace WebApp.Api.Infrastructure
{
    public class StatusCodeWithReasonResult : StatusCodeResult
    {
        public StatusCodeWithReasonResult([ActionResultStatusCode] int statusCode, string reason) : base(statusCode)
        {
            Reason = reason;
        }

        public string Reason { get; }

        public override void ExecuteResult(ActionContext context)
        {
            base.ExecuteResult(context);

            var httpResponseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
            if (httpResponseFeature != null)
                httpResponseFeature.ReasonPhrase = Reason;
        }
    }
}
