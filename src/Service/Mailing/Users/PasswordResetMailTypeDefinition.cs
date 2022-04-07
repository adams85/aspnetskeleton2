using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Mailing.Users;

internal sealed class PasswordResetMailTypeDefinition : MailTypeDefinition<PasswordResetMailModel, PasswordResetMailMessageProducer>
{
    public override string MailType => PasswordResetMailModel.AssociatedMailType;

    public override Task ValidateModelAsync(MailModelValidationContext context, CancellationToken cancellationToken)
    {
        if (context.Model is not PasswordResetMailModel model)
            context.SetError(ServiceErrorCode.ParamNotValid);

        return Task.CompletedTask;
    }
}
