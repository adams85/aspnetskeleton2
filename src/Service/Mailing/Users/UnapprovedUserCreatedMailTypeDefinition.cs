using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Mailing.Users;

internal sealed class UnapprovedUserCreatedMailTypeDefinition : MailTypeDefinition<UnapprovedUserCreatedMailModel, UnapprovedUserCreatedMailMessageProducer>
{
    public override string MailType => UnapprovedUserCreatedMailModel.AssociatedMailType;

    public override Task ValidateModelAsync(MailModelValidationContext context, CancellationToken cancellationToken)
    {
        if (context.Model is not UnapprovedUserCreatedMailModel model)
            context.SetError(ServiceErrorCode.ParamNotValid);

        return Task.CompletedTask;
    }
}
