using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure;

internal delegate Task<object?> QueryExecutionDelegate(QueryContext context, CancellationToken cancellationToken);

internal delegate IQueryInterceptor QueryInterceptorFactory(IServiceProvider serviceProvider, QueryExecutionDelegate next);

internal interface IQueryInterceptor { }
