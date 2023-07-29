using System;
using System.Reactive.Linq;

namespace CherukPhotoEditor.View.Utils.Extensions;

public static class ObservableExtensions
{
    public static IObservable<T> TakeWhenNot<T>(this IObservable<T> source, IObservable<bool> predicate)
    {
        return source.WithLatestFrom(predicate)
            .Where(args => !args.Second)
            .Select(x => x.First);
    }
    
    public static IObservable<T> TakeWhen<T>(this IObservable<T> source, IObservable<bool> predicate)
    {
        return source.WithLatestFrom(predicate)
            .Where(args => args.Second)
            .Select(x => x.First);
    }
}