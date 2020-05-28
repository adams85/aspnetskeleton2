using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure.Validation
{
    internal sealed class CommandDataAnnotationsValidatorInterceptor
    {
        private readonly CommandExecutionDelegate _next;

        public CommandDataAnnotationsValidatorInterceptor(CommandExecutionDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(CommandContext context, CancellationToken cancellationToken)
        {
            try { DataAnnotationsValidator.Validate(context.Command, context.ScopedServices); }
            catch (ValidationException ex) { throw ServiceErrorException.From(ex); }

            return _next(context, cancellationToken);
        }
    }
}
