using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Mailing.Users
{
    internal sealed class UserLockedOutMailTypeDefinition : MailTypeDefinition<UserLockedOutMailModel, UserLockedOutMailMessageProducer>
    {
        public override string MailType => UserLockedOutMailModel.AssociatedMailType;

        public override Task ValidateModelAsync(MailModelValidationContext context, CancellationToken cancellationToken)
        {
            if (!(context.Model is UserLockedOutMailModel model))
                context.SetError(ServiceErrorCode.ParamNotValid);

            return Task.CompletedTask;
        }
    }
}
