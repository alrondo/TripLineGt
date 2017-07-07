using System;
using System.Collections;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace TripLine.Toolbox
{
    public static class ObservableEx
    {
        public static IObservable<T> OnFunc<T>(this IObservable<T> source, Func<T, bool> delegateFunc)
        {
            return source.OnFunc(delegateFunc, Scheduler.Default);
        }

        public static IObservable<T> OnFunc<T>(this IObservable<T> source, Func<T, bool> delegateFunc, IScheduler scheduler)
        {
            return Observable.Create<T>(o =>
            {
                T value = default(T);

                Action action = () =>
                {
                    o.OnNext(value);
                };

                return source.Subscribe(
                    x =>
                    {
                        value = x;
                        if (delegateFunc(x))
                        {

                            scheduler.Schedule(action);
                        }

                    },
                    o.OnError,
                    o.OnCompleted
                    );

            });
        }

        public static IObservable<T> OnFuncOrTimeout<T>(this IObservable<T> source, Func<T, bool> delegateFunc, TimeSpan span)
        {
            return source.OnFuncOrTimeout(delegateFunc, span, Scheduler.Default);
        }

        public static IObservable<T> OnFuncOrTimeout<T>(this IObservable<T> source, Func<T, bool> delegateFunc, TimeSpan span, IScheduler scheduler)
        {
            return Observable.Create<T>(o =>
            {
                var hasValue = false;
                T value = default(T);

                var timeoutDisposable = new SerialDisposable();
                Action action = null; 
                action = () =>
                {
                    if (hasValue)
                    {
                        timeoutDisposable.Disposable = System.Reactive.Disposables.Disposable.Empty;
                        o.OnNext(value);
                        hasValue = false;
                        timeoutDisposable.Disposable = scheduler.Schedule(span, action);

                    }
                };
                timeoutDisposable.Disposable = scheduler.Schedule(span, action);


                return source.Subscribe(
                    x =>
                    {
                        //if (!hasValue)
                        //{
                        //    timeoutDisposable.Disposable = scheduler.Schedule(span, action);
                        //}

                        value = x;
                        hasValue = true;
                        if (delegateFunc(x))
                        {
                            scheduler.Schedule(action);
                        }
                    },
                    o.OnError,
                    o.OnCompleted
                    );

            });
        }


    }
}