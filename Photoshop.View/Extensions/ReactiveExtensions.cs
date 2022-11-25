using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using ReactiveUI;

namespace Photoshop.View.Extensions;

public static class ReactiveExtensions
{
    public static IObservable<TValue> ObservableForPropertyValue<TSender, TValue>(
        this TSender sender,
        Expression<Func<TSender, TValue>> property)
    {
        return sender.ObservableForProperty(property, skipInitial: false).Select(x => x.Value);
    }
}