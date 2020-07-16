using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Karambolo.Common;
using ProtoBuf.Grpc;
using WebApp.Service.Host.Models;
using WebApp.Service.Infrastructure;

namespace WebApp.Service.Host.Services
{
    public sealed class CommandService : ICommandService
    {
        private static readonly string s_serviceContractAssemblyName = typeof(ICommand).Assembly.GetName().Name!;

        private static readonly UnboundedChannelOptions s_eventChannelOptions = new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        };

        private readonly ICommandDispatcher _commandDispatcher;

        public CommandService(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        }

        private async IAsyncEnumerable<CommandResponse> InvokeCore(CommandRequest request, bool relayEvents, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (request.CommandTypeName == null)
                throw new ArgumentException("Command type is not specified.", nameof(request));

            var commandType = Type.GetType(request.CommandTypeName + "," + s_serviceContractAssemblyName, throwOnError: true)!;

            if (!commandType.HasInterface(typeof(ICommand)))
                throw new ArgumentException("Command type is invalid.", nameof(request));

            var command = (ICommand)ServiceHostContractSerializer.Default.Deserialize(request.SerializedCommand ?? Array.Empty<byte>(), commandType);

            ServiceErrorException? errorException = null;

            object? key = null;
            Action<ICommand, object>? handleKeyGenerated;

            var keyGeneratorCommand = command as IKeyGeneratorCommand;
            if (keyGeneratorCommand != null)
            {
                handleKeyGenerated = (_, k) => key = k;
                keyGeneratorCommand.OnKeyGenerated += handleKeyGenerated;
            }
            else
                handleKeyGenerated = null;

            try
            {
                Task dispatchTask;

                if (relayEvents && command is IEventProducerCommand eventProducerCommand)
                {
                    var eventChannel = Channel.CreateUnbounded<Event>(s_eventChannelOptions);

                    eventProducerCommand.OnEvent = (_, @event) => eventChannel.Writer.TryWrite(@event);

                    dispatchTask = Task.Run(async () =>
                    {
                        try { await _commandDispatcher.DispatchAsync(command, cancellationToken); }
                        finally { eventChannel.Writer.Complete(); }
                    });

                    while (await eventChannel.Reader.WaitToReadAsync(cancellationToken))
                        while (eventChannel.Reader.TryRead(out var @event))
                            yield return new CommandResponse.Notification
                            {
                                Event = new EventData { Value = @event }
                            };
                }
                else
                    dispatchTask = _commandDispatcher.DispatchAsync(command, cancellationToken);

                try { await dispatchTask; }
                catch (ServiceErrorException ex) { errorException = ex; }
            }
            finally
            {
                if (keyGeneratorCommand != null)
                    keyGeneratorCommand.OnKeyGenerated -= handleKeyGenerated;
            }

            if (errorException != null)
                yield return new CommandResponse.Failure { Error = errorException.ToData() };
            else
                yield return new CommandResponse.Success { Key = key != null ? KeyData.From(key) : null };
        }

        public async ValueTask<CommandResponse> Invoke(CommandRequest request, CallContext context = default)
        {
            await using var enumerator = InvokeCore(request, relayEvents: false, context.CancellationToken).GetAsyncEnumerator();

            if (!await enumerator.MoveNextAsync())
                throw new InvalidOperationException();

            return enumerator.Current;
        }

        public IAsyncEnumerable<CommandResponse> InvokeWithEventNotification(CommandRequest request, CallContext context = default) =>
            InvokeCore(request, relayEvents: true, context.CancellationToken);
    }
}
