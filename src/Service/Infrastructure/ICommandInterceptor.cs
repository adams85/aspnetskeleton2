using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure;

internal delegate Task CommandExecutionDelegate(CommandContext context, CancellationToken cancellationToken);

internal delegate ICommandInterceptor CommandInterceptorFactory(IServiceProvider serviceProvider, CommandExecutionDelegate next);

internal interface ICommandInterceptor { }
