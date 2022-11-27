using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Photoshop.Domain;
using Photoshop.View.Utils.Extensions;
using ReactiveUI;

namespace Photoshop.View.ViewModels;

public class ColorSpaceContext : ReactiveObject, IDisposable
{
    private ColorSpace _currentColorSpace;

    private List<IDisposable> _subscriptions = new();

    public ColorSpaceContext()
    {
        ChannelCollection = new ObservableCollection<Channel>(new List<Channel>
        {
            new("Red", true),
            new("Green", true),
            new("Blue", true)
        });

        this.ObservableForPropertyValue(x => x.CurrentColorSpace)
            .Subscribe(_ => OnColorSpaceChanged())
            .AddTo(_subscriptions);

        Channels = ChannelCollection
            .Select(x => x.ObservableForPropertyValue(y => y.Value))
            .CombineLatest(x => new List<bool>(x).ToArray());
    }
    
    public IObservable<bool[]> Channels { get; }

    public ColorSpace CurrentColorSpace
    {
        get => _currentColorSpace;
        set => this.RaiseAndSetIfChanged(ref _currentColorSpace, value);
    }
    
    private ObservableCollection<Channel> ChannelCollection { get; }

    private ColorSpace[] ColorSpaces { get; } = Enum.GetValues<ColorSpace>();

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

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}