using System;
using System.Collections.Generic;

namespace WebApp.Service.Infrastructure
{
    internal class QueryDispatcherOptions
    {
        public List<(Predicate<Type> QueryTypeFilter, QueryInterceptorFactory InterceptorFactory)>? InterceptorFactories { get; set; }
    }
}
