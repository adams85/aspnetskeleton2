using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace WebApp.Service.Mailing;

internal interface IMailMessageProducer
{
    Task<MimeMessage> ProduceAsync(MailModel model, CancellationToken cancellationToken);
}
