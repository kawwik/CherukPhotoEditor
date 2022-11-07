using System;
using Avalonia.Controls;
using Photoshop.Domain;
using ReactiveUI;

namespace Photoshop.View.ViewModels;

public class ColorSpaceContext : ReactiveObject
{
    public ColorSpaceContext(ComboBox colorSpaceComboBox)
    {
        ColorSpaceComboBox = colorSpaceComboBox;
        ColorSpaceComboBox.Items = Enum.GetValues<ColorSpace>();
        ColorSpaceComboBox.SelectedItem = ColorSpace.Rgb;
        
        SetColorChannelNames();
    }

    private string FirstChannelName { get; set; } = null!;
    private string SecondChannelName { get; set; } = null!;
    private string ThirdChannelName { get; set; } = null!;

    public static string ColorSpaceComboBoxName => "ColorSpace";

    public ColorSpace CurrentColorSpace => (ColorSpace)ColorSpaceComboBox.SelectedItem!;

    public ComboBox ColorSpaceComboBox { get; }

    private void SetColorChannelNames()
    {
        (FirstChannelName, SecondChannelName, ThirdChannelName) = CurrentColorSpace switch
        {
            ColorSpace.Rgb => ("Red", "Green", "Blue"),
            _ => ("Nan", "Nan", "Nan")
        };
    }
}