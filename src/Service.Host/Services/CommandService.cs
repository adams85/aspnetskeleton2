using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Karambolo.Common;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Internal;
using WebApp.Service.Host;
using WebApp.Service.Host.Models;
using WebApp.Service.Host.Services;
using WebApp.Service.Infrastructure;

namespace WebApp.Service.Host.Services
{
    public sealed class CommandService : ICommandService
    {
        private static readonly string s_serviceContractAssemblyName = typeof(ICommand).Assembly.GetName().Name!;

        private readonly ICommandDispatcher _commandDispatcher;

        public CommandService(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        }

        private async IAsyncEnumerable<CommandResponse> InvokeCore(CommandRequest request, bool notifyEvents, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

                if (notifyEvents && command is IEventProducerCommand eventProducerCommand)
                {
                    var channel = Channel.CreateUnbounded<Event>(new UnboundedChannelOptions
                    {
                        SingleReader = true,
                        SingleWriter = false,
                    });

                    eventProducerCommand.OnEvent = (_, @event) => channel.Writer.TryWrite(@event);

                    dispatchTask = Task.Run(async () =>
                    {
                        try { await _commandDispatcher.DispatchAsync(command, cancellationToken); }
                        finally { channel.Writer.Complete(); }
                    });

                    while (await channel.Reader.WaitToReadAsync(cancellationToken))
                        while (channel.Reader.TryRead(out var @event))
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
            await using var enumerator = InvokeCore(request, notifyEvents: false, context.CancellationToken).GetAsyncEnumerator();

            if (!await enumerator.MoveNextAsync())
                throw new InvalidOperationException();

            return enumerator.Current;
        }

        public IAsyncEnumerable<CommandResponse> InvokeWithEventNotification(CommandRequest request, CallContext context = default) =>
            InvokeCore(request, notifyEvents: true, context.CancellationToken);
    }
}
