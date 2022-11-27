using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using Photoshop.Domain;
using Photoshop.View.Extensions;
using ReactiveUI;

namespace Photoshop.View.ViewModels;

public class Channel : ReactiveObject
{
    private string _name;
    private bool _value;

    public Channel(string name, bool value)
    {
        Name = name;
        Value = value;
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public bool Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
}

public class ReactiveCollection<T> : ObservableCollection<T> where T : ReactiveObject
{
    public ReactiveCollection(List<T> items) : base(items)
    {
        items.ForEach(x => base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
    }
}

public class ColorSpaceContext : ReactiveObject
{
    private ColorSpace _currentColorSpace;

    public ColorSpaceContext()
    {
        ChannelCollection = new ReactiveCollection<Channel>(new List<Channel>
        {
            new("Red", true),
            new("Green", true),
            new("Blue", true)
        });
        
        this.ObservableForPropertyValue(x => x.CurrentColorSpace)
            .Subscribe(_ => OnColorSpaceChanged());

        Channels = Observable.CombineLatest(
            ChannelCollection.Select(x => x.ObservableForPropertyValue(y => y.Value)),
            x => new List<bool>(x).ToArray()
        );
    }
    
    private ReactiveCollection<Channel> ChannelCollection { get; }

    private ColorSpace[] ColorSpaces { get; } = Enum.GetValues<ColorSpace>();
    public IObservable<bool[]> Channels { get; }

    public ColorSpace CurrentColorSpace
    {
        get => _currentColorSpace;
        set => this.RaiseAndSetIfChanged(ref _currentColorSpace, value);
    }

    private void OnColorSpaceChanged()
    {
        (ChannelCollection[0].Name, ChannelCollection[1].Name, ChannelCollection[2].Name) = CurrentColorSpace switch
        {
            ColorSpace.Rgb => ("Red", "Green", "Blue"),
            ColorSpace.Hsl => ("Hue", "Saturation", "Lightness"),
            ColorSpace.Hsv => ("Hue", "Saturation", "Value"),
            ColorSpace.YCbCr601 => ("Y", "Cb", "Cr"),
            ColorSpace.YCbCr709 => ("Y", "Cb", "Cr"),
            ColorSpace.YCoCg => ("Y", "Co", "Cg"),
            ColorSpace.Cmy => ("Cyan", "Magenta", "Yellow"),
            _ => ("Nan", "Nan", "Nan")
        };
    }
}