using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using WebApp.Service.Host.Models;

namespace WebApp.Service.Host.Services;

[ServiceContract]
public interface IQueryService
{
    [OperationContract]
    ValueTask<QueryResponse> Invoke(QueryRequest request, CallContext context = default);

    [OperationContract]
    IAsyncEnumerable<QueryResponse> InvokeWithEventNotification(QueryRequest request, CallContext context = default);
}
