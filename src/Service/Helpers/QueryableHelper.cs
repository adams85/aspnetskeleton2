using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WebApp.Service.Helpers;

public delegate IOrderedQueryable<T> ApplyOrderByElement<T>(IQueryable<T> source, string keyPropertyPath, bool descending, bool nested);

public static class QueryableHelper
{
    private static readonly MethodInfo s_orderByMethodDefinition =
        new Func<IQueryable<object>, Expression<Func<object, object>>, object>(Queryable.OrderBy<object, object>).Method.GetGenericMethodDefinition();

    private static readonly MethodInfo s_orderByDescendingMethodDefinition =
        new Func<IQueryable<object>, Expression<Func<object, object>>, object>(Queryable.OrderByDescending<object, object>).Method.GetGenericMethodDefinition();

    private static readonly MethodInfo s_thenByMethodDefinition =
        new Func<IOrderedQueryable<object>, Expression<Func<object, object>>, object>(Queryable.ThenBy<object, object>).Method.GetGenericMethodDefinition();

    private static readonly MethodInfo s_thenByDescendingMethodDefinition =
        new Func<IOrderedQueryable<object>, Expression<Func<object, object>>, object>(Queryable.ThenByDescending<object, object>).Method.GetGenericMethodDefinition();

    private static IOrderedQueryable<T> OrderByCore<T>(this IQueryable<T> source, string keyPropertyPath, MethodInfo orderMethodDefinition)
    {
        var type = typeof(T);
        var @param = Expression.Parameter(type);

        Expression propertyAccess = @param;
        var propertyNames = keyPropertyPath.Split('.');
        for (int i = 0, n = propertyNames.Length; i < n; i++)
            propertyAccess = Expression.Property(propertyAccess, propertyNames[i]);

        var keySelector = Expression.Lambda(propertyAccess, @param);

        var orderMethod = orderMethodDefinition.MakeGenericMethod(type, propertyAccess.Type);

        var orderQuery = Expression.Call(orderMethod, source.Expression, Expression.Quote(keySelector));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(orderQuery);
    }

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string keyPropertyPath, bool descending = false)
    {
        return source.OrderByCore(keyPropertyPath, !descending ? s_orderByMethodDefinition : s_orderByDescendingMethodDefinition);
    }

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string keyPropertyPath, bool descending = false)
    {
        return source.OrderByCore(keyPropertyPath, !descending ? s_thenByMethodDefinition : s_thenByDescendingMethodDefinition);
    }

    public static (string KeyPropertyPath, bool Descending) ParseOrderByElement(string value)
    {
        var c = value[0];
        switch (c)
        {
            case '+':
            case '-':
                return (value.Substring(1), c == '-');
            default:
                return (value, false);
        }
    }

    public static string ComposeOrderByElement(string keyPropertyPath, bool descending)
    {
        return descending ? "-" + keyPropertyPath : keyPropertyPath;
    }

    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> source, ApplyOrderByElement<T> applyOrderByElement, params string[] orderByElements)
    {
        for (int i = 0, n = orderByElements.Length; i < n; i++)
        {
            var orderByElement = orderByElements[i];

            if (string.IsNullOrEmpty(orderByElement))
                throw new ArgumentException("Elements cannot be null or empty.", nameof(orderByElements));

            var (keyPropertyPath, descending) = ParseOrderByElement(orderByElement);
            source = applyOrderByElement(source, keyPropertyPath, descending, nested: i > 0);
        }

        return source;
    }

    public static int GetEffectivePageSize(int pageSize, int maxPageSize)
    {
        return maxPageSize > 0 ? Math.Min(pageSize, maxPageSize) : pageSize;
    }

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> source, int pageIndex, int pageSize, int maxPageSize)
    {
        if (pageIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(pageIndex));

        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        pageSize = GetEffectivePageSize(pageSize, maxPageSize);

        return source.Skip(pageIndex * pageSize).Take(pageSize);
    }
}
