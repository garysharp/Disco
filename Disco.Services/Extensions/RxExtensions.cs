using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Disco
{
    public static class RxExtensions
    {

        #region RxExtensions

        public static IObservable<IEnumerable<T>> DelayBuffer<T>(this IObservable<T> source, TimeSpan delay)
        {
            return Observable.Create<IEnumerable<T>>(o =>
            {
                var gate = new object();
                var buffer = new List<T>();
                var trigger = (IDisposable)null;
                var subscription = (IDisposable)null;
                var scheduler = Scheduler.Default;

                Action dump = () =>
                {
                    var bts = buffer.ToArray();
                    buffer = new List<T>();
                    if (o != null)
                        o.OnNext(bts);
                    if (trigger != null)
                    {
                        trigger.Dispose();
                        trigger = null;
                    }
                };

                Action dispose = () =>
                {
                    if (subscription != null)
                        subscription.Dispose();
                    if (trigger != null)
                    {
                        trigger.Dispose();
                        trigger = null;
                    }
                };

                Action<Action<IObserver<IEnumerable<T>>>> onErrorOrCompleted =
                    onAction =>
                    {
                        lock (gate)
                        {
                            dispose();
                            dump();
                            if (o != null)
                                onAction(o);
                        }
                    };

                Action<Exception> onError = ex =>
                    onErrorOrCompleted(x => x.OnError(ex));

                Action onCompleted = () => onErrorOrCompleted(x => x.OnCompleted());

                Action<T> onNext = t =>
                {
                    lock (gate)
                    {
                        buffer.Add(t);

                        if (trigger == null)
                        {
                            trigger = scheduler.Schedule(delay, () =>
                            {
                                lock (gate)
                                {
                                    dump();
                                }
                            });
                        }
                    }
                };

                subscription =
                    source
                        .ObserveOn(scheduler)
                        .Subscribe(onNext, onError, onCompleted);

                return () =>
                {
                    lock (gate)
                    {
                        o = null;
                        dispose();
                    }
                };
            });
        }

        public static IObservable<IEnumerable<T>> BufferWithInactivity<T>(this IObservable<T> source, TimeSpan inactivity)
        {
            return Observable.Create<IEnumerable<T>>(o =>
            {
                var gate = new object();
                var buffer = new List<T>();
                var mutable = new SerialDisposable();
                var subscription = (IDisposable)null;
                var scheduler = Scheduler.Default;

                Action dump = () =>
                {
                    var bts = buffer.ToArray();
                    buffer = new List<T>();
                    if (o != null)
                    {
                        o.OnNext(bts);
                    }
                };

                Action dispose = () =>
                {
                    if (subscription != null)
                    {
                        subscription.Dispose();
                    }
                    mutable.Dispose();
                };

                Action<Action<IObserver<IEnumerable<T>>>> onErrorOrCompleted =
                    onAction =>
                    {
                        lock (gate)
                        {
                            dispose();
                            dump();
                            if (o != null)
                            {
                                onAction(o);
                            }
                        }
                    };

                Action<Exception> onError = ex =>
                    onErrorOrCompleted(x => x.OnError(ex));

                Action onCompleted = () => onErrorOrCompleted(x => x.OnCompleted());

                Action<T> onNext = t =>
                {
                    lock (gate)
                    {
                        buffer.Add(t);
                        mutable.Disposable = scheduler.Schedule(inactivity, () =>
                        {
                            lock (gate)
                            {
                                dump();
                            }
                        });
                    }
                };

                subscription =
                    source
                        .ObserveOn(scheduler)
                        .Subscribe(onNext, onError, onCompleted);

                return () =>
                {
                    lock (gate)
                    {
                        o = null;
                        dispose();
                    }
                };
            });
        }

        #endregion

    }
}
