using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace WebApp.Service.Helpers
{
    public static class ObservableHelper
    {
        public static IObservable<TSource> RetryAfterDelay<TSource>(this IObservable<TSource> source, TimeSpan dueTime) =>
            source.RetryAfterDelay(dueTime, DefaultScheduler.Instance);

        // based on: https://stackoverflow.com/questions/18978523/write-an-rx-retryafter-extension-method#answer-20145295
        public static IObservable<TSource> RetryAfterDelay<TSource>(this IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
        {
            return RepeateInfinitelyWithDelay(source, dueTime, scheduler).Catch();

            static IEnumerable<IObservable<TSource>> RepeateInfinitelyWithDelay(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
            {
                yield return source;

                while (true)
                    yield return source.DelaySubscription(dueTime, scheduler);
            }
        }
    }
}
