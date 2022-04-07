using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace WebApp.Service;

internal partial class QueryContext
{
    private static readonly ConcurrentDictionary<Type, Type> s_resultTypes = new ConcurrentDictionary<Type, Type>();

    public static Type GetResultType(Type queryType)
    {
        return s_resultTypes.GetOrAdd(queryType, type =>
        {
            Type? interfaceType;
            try { interfaceType = type.GetInterface(typeof(IQuery<>).FullName!); }
            catch (AmbiguousMatchException) { interfaceType = null; }

            if (interfaceType == null)
                throw new ArgumentException($"Query type {type} implements no or multiple {typeof(IQuery<>)} interfaces.", nameof(type));

            return interfaceType.GetGenericArguments()[0];
        });
    }
}
