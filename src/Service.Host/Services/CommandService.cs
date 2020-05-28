using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using ProtoBuf.Grpc;
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

        private async IAsyncEnumerable<CommandResponse> InvokeCore(CommandRequest request, bool reportProgress, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                if (reportProgress && command is IProgressReporterCommand progressReporterCommand)
                {
                    using (var progressEventSubject = new Subject<ProgressEventData>())
                    {
                        progressReporterCommand.Progress = new Progress<ProgressEventData>(@event => progressEventSubject.OnNext(@event));

                        var dispatchStatus = Observable.FromAsync(() => _commandDispatcher.DispatchAsync(command, cancellationToken))
                            .Select(_ => true)
                            .StartWith(false);

                        var progressEvents = progressEventSubject
                            .StartWith(default(ProgressEventData)!)
                            .CombineLatest(dispatchStatus, (@event, status) => (@event, executed: status))
                            .Skip(1);

                        await using (var enumerator = progressEvents.ToAsyncEnumerable().GetAsyncEnumerator(cancellationToken))
                        {
                            for (; ; )
                            {
                                bool success;
                                try { success = await enumerator.MoveNextAsync(); }
                                catch (ServiceErrorException ex)
                                {
                                    errorException = ex;
                                    break;
                                }

                                if (!success || enumerator.Current.executed)
                                    break;

                                yield return new CommandResponse.Progress { Event = enumerator.Current.@event };
                            }
                        }
                    }
                }
                else
                    try { await _commandDispatcher.DispatchAsync(command, cancellationToken); }
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

        public ValueTask<CommandResponse> Invoke(CommandRequest request, CallContext context = default)
        {
            return InvokeCore(request, reportProgress: false, context.CancellationToken).SingleAsync();
        }

        public IAsyncEnumerable<CommandResponse> InvokeWithProgressReporting(CommandRequest request, CallContext context = default)
        {
            return InvokeCore(request, reportProgress: true, context.CancellationToken);
        }
    }
}
