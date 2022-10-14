using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Photoshop.View.Windows;

public partial class ErrorDialog : Window
{
    public ErrorDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}