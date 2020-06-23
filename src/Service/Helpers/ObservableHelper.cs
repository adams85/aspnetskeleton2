using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Karambolo.Common;

namespace WebApp.Service.Helpers
{
    public static class ObservableHelper
    {
        public static IObservable<TSource> DoOnSubscribe<TSource>(this IObservable<TSource> source, Action action) =>
            Observable.Create<TSource>(observer =>
            {
                action();
                return source.SubscribeSafe(observer);
            });

        public static IObservable<TSource> Retry<TSource>(this IObservable<TSource> source,
            Func<IObservable<TSource>, IObservable<TSource>>? wrapInitial = null, Func<IObservable<TSource>, IObservable<TSource>>? wrapSubsequent = null)
        {
            return RepeateInfinitely(source, wrapInitial ?? Identity<IObservable<TSource>>.Func, wrapSubsequent ?? Identity<IObservable<TSource>>.Func)
                .Catch();

            static IEnumerable<IObservable<TSource>> RepeateInfinitely(IObservable<TSource> source,
                Func<IObservable<TSource>, IObservable<TSource>> wrapInitial, Func<IObservable<TSource>, IObservable<TSource>> wrapSubsequent)
            {
                yield return wrapInitial(source);

                for (; ; )
                    yield return wrapSubsequent(source);
            }
        }

        public static IObservable<TSource> Retry<TSource>(this IObservable<TSource> source, int retryCount,
            Func<IObservable<TSource>, IObservable<TSource>>? wrapInitial = null, Func<IObservable<TSource>, IObservable<TSource>>? wrapSubsequent = null)
        {
            if (retryCount < 0)
                throw new ArgumentOutOfRangeException(nameof(retryCount));

            return Repeate(source, retryCount, wrapInitial ?? Identity<IObservable<TSource>>.Func, wrapSubsequent ?? Identity<IObservable<TSource>>.Func)
                .Catch();

            static IEnumerable<IObservable<TSource>> Repeate(IObservable<TSource> source, int retryCount,
                Func<IObservable<TSource>, IObservable<TSource>> wrapInitial, Func<IObservable<TSource>, IObservable<TSource>> wrapSubsequent)
            {
                yield return wrapInitial(source);

                for (; retryCount > 0; retryCount--)
                    yield return wrapSubsequent(source);
            }
        }
    }
}
