using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using WebApp.DataAccess;

namespace WebApp.Service.Mailing;

internal interface IMailSenderService : IHostedService
{
    Task EnqueueItemAsync(MailModel model, WritableDataContext dbContext, IChangeToken? transactionCommittedToken, CancellationToken cancellationToken);
}
