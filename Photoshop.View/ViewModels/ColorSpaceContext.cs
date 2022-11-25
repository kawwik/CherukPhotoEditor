using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Avalonia.Controls;
using Photoshop.Domain;
using Photoshop.View.Extensions;
using ReactiveUI;

namespace Photoshop.View.ViewModels;

public class ColorSpaceContext : ReactiveObject
{
    private string _firstChannelName = null!;
    private string _secondChannelName = null!;
    private string _thirdChannelName = null!;
    private bool _firstChannelValue = true;
    private bool _secondChannelValue = true;
    private bool _thirdChannelValue = true;

    public ColorSpaceContext(ComboBox colorSpaceComboBox)
    {
        ColorSpaceComboBox = colorSpaceComboBox;
        InitializeComboBox();

        Channels = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.FirstChannelValue),
            this.ObservableForPropertyValue(x => x.SecondChannelValue),
            this.ObservableForPropertyValue(x => x.ThirdChannelValue),
            (first, second, third) => new []{first, second, third}
        );
    }

    private void InitializeComboBox()
    {
        ColorSpaceComboBox.Items = Enum.GetValues<ColorSpace>();
        ColorSpaceComboBox.SelectionChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(CurrentColorSpace));
            OnColorSpaceChanged();
        };
        ColorSpaceComboBox.SelectedItem = ColorSpace.Rgb;
    }

    private string FirstChannelName
    {
        get => _firstChannelName;
        set => this.RaiseAndSetIfChanged(ref _firstChannelName, value);
    }

    private string SecondChannelName
    {
        get => _secondChannelName;
        set => this.RaiseAndSetIfChanged(ref _secondChannelName, value);
    }

    private string ThirdChannelName
    {
        get => _thirdChannelName;
        set => this.RaiseAndSetIfChanged(ref _thirdChannelName, value);
    }

    private bool FirstChannelValue
    {
        get => _firstChannelValue;
        set => this.RaiseAndSetIfChanged(ref _firstChannelValue, value);
    }

    private bool SecondChannelValue
    {
        get => _secondChannelValue;
        set => this.RaiseAndSetIfChanged(ref _secondChannelValue, value);
    }

    private bool ThirdChannelValue
    {
        get => _thirdChannelValue;
        set => this.RaiseAndSetIfChanged(ref _thirdChannelValue, value);
    }

    public IObservable<bool[]> Channels { get; }

    public static string ColorSpaceComboBoxName => "ColorSpace";

    public ColorSpace CurrentColorSpace => (ColorSpace)ColorSpaceComboBox.SelectedItem!;

    public ComboBox ColorSpaceComboBox { get; }

    private void OnColorSpaceChanged()
    {
        (FirstChannelName, SecondChannelName, ThirdChannelName) = CurrentColorSpace switch
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