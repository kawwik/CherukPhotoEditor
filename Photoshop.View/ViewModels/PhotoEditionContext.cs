using System.Windows.Input;
using Photoshop.View.Commands;

namespace Photoshop.View.ViewModels;

public class PhotoEditionContext
{
    public ICommand OpenImage { get; } = new OpenImageCommand();
    public ICommand SaveImage { get; } = new SaveImageCommand();
}