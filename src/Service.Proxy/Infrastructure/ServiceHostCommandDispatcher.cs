using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ProtoBuf.Grpc;
using WebApp.Core.Helpers;
using WebApp.Service.Host;
using WebApp.Service.Host.Models;
using WebApp.Service.Host.Services;

namespace WebApp.Service.Infrastructure;

internal sealed class ServiceHostCommandDispatcher : ICommandDispatcher
{
    private readonly ICommandService _commandService;

    public ServiceHostCommandDispatcher(IServiceHostGrpcServiceFactory serviceFactory)
    {
        _commandService = serviceFactory.CreateGrpcService<ICommandService>();
    }

    public async Task DispatchAsync(ICommand command, CancellationToken cancellationToken)
    {
        var commandType = command.GetType();

        var commandRequest = new CommandRequest
        {
            CommandTypeName = commandType.FullNameWithoutAssemblyDetails(),
            SerializedCommand = ServiceHostContractSerializer.Default.Serialize(command, commandType)
        };

        var callContext = new CallContext(new CallOptions(cancellationToken: cancellationToken));

        CommandResponse? response = null;

        if (command is IEventProducerCommand eventProducerCommand && eventProducerCommand.OnEvent != null)
        {
            await foreach (var currentResponse in _commandService.InvokeWithEventNotification(commandRequest, callContext).ConfigureAwait(false))
            {
                if (currentResponse is CommandResponse.Notification notificationResponse)
                    eventProducerCommand.OnEvent.Invoke(command, notificationResponse.Event.Value);

                response = currentResponse;
            }
        }
        else
            response = await _commandService.Invoke(commandRequest, callContext).ConfigureAwait(false);

        switch (response)
        {
            case CommandResponse.Success successResponse:
                if (command is IKeyGeneratorCommand keyGeneratorCommand && successResponse.Key?.Value != null)
                    keyGeneratorCommand.OnKeyGenerated?.Invoke(command, successResponse.Key.Value);

                return;
            case CommandResponse.Failure failureResponse:
                throw ServiceErrorException.From(failureResponse.Error);
            case null:
                throw new InvalidOperationException("No command response was received.");
            default:
                throw new InvalidOperationException($"A command response of unexpected type {response.GetType()} was received.");
        }
    }
}
