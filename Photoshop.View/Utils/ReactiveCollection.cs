using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ReactiveUI;

namespace Photoshop.View.Utils;

public class ReactiveCollection<T> : ObservableCollection<T> where T : ReactiveObject
{
    public ReactiveCollection(List<T> items) : base(items)
    {
        items.ForEach(x => base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
    }
}