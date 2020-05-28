using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Mailing
{
    internal interface IMailTypeDefinition
    {
        string MailType { get; }

        Task ValidateModelAsync(MailModelValidationContext context, CancellationToken cancellationToken);

        byte[] SerializeModel(MailModel model);
        MailModel DeserializeModel(byte[] bytes);

        IMailMessageProducer CreateMailMessageProducer(IServiceProvider serviceProvider);
    }
}
