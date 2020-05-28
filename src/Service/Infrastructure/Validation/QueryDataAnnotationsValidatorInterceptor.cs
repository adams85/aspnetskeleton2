using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure.Validation
{
    internal sealed class QueryDataAnnotationsValidatorInterceptor
    {
        private readonly QueryExecutionDelegate _next;

        public QueryDataAnnotationsValidatorInterceptor(QueryExecutionDelegate next)
        {
            _next = next;
        }

        public Task<object?> InvokeAsync(QueryContext context, CancellationToken cancellationToken)
        {
            try { DataAnnotationsValidator.Validate(context.Query, context.ScopedServices); }
            catch (ValidationException ex) { throw ServiceErrorException.From(ex); }

            return _next(context, cancellationToken);
        }
    }
}
