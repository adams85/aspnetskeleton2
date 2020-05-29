using System;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;

namespace WebApp.Service.Mailing
{
    internal sealed class EnqueueMailCommandHandler : CommandHandler<EnqueueMailCommand>
    {
        private readonly IMailTypeCatalog _mailTypeCatalog;
        private readonly IMailSenderService _mailSenderService;

        public EnqueueMailCommandHandler(IMailTypeCatalog mailTypeCatalog, IMailSenderService mailSenderService)
        {
            _mailTypeCatalog = mailTypeCatalog ?? throw new ArgumentNullException(nameof(mailTypeCatalog));
            _mailSenderService = mailSenderService ?? throw new ArgumentNullException(nameof(mailSenderService));
        }

        public override async Task HandleAsync(EnqueueMailCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            var mailTypeDefinition = _mailTypeCatalog.GetDefinition(command.Model.MailType);
            RequireValid(mailTypeDefinition != null, c => c.Model.MailType);

            var validationContext = new MailModelValidationContext(() => Lambda.MemberPath((EnqueueMailCommand c) => c.Model))
            {
                Model = command.Model,
                DbContext = context.DbContext,
            };

            await mailTypeDefinition!.ValidateModelAsync(validationContext, cancellationToken).ConfigureAwait(false);

            if (validationContext.ErrorCode != null)
                new ServiceErrorException(validationContext.ErrorCode.Value, validationContext.ErrorArgsFactory?.Invoke());

            await _mailSenderService.EnqueueItemAsync(command.Model, context.DbContext, null, cancellationToken).ConfigureAwait(false);
        }
    }
}
