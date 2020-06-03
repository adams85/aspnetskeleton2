using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using WebApp.Service.Host.Models;

namespace WebApp.Service.Host.Services
{
    [ServiceContract]
    public interface ICommandService
    {
        [OperationContract]
        ValueTask<CommandResponse> Invoke(CommandRequest request, CallContext context = default);

        [OperationContract]
        IAsyncEnumerable<CommandResponse> InvokeWithEventNotification(CommandRequest request, CallContext context = default);
    }
}
