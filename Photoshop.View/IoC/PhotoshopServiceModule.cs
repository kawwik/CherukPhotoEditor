using Avalonia.Controls;
using Ninject.Modules;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.View.Converters;
using Photoshop.View.Services;
using Photoshop.View.Services.Interfaces;

namespace Photoshop.View.IoC;

public class PhotoshopServiceModule : NinjectModule
{
    private readonly Window _mainWindow;

    public PhotoshopServiceModule(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public override void Load()
    {
        Bind<IDialogService>().To<DialogService>().WithConstructorArgument("parentWindow", _mainWindow);
        Bind<IImageConverter>().To<ImageConverter>();
        Bind<IImageEditorFactory>().To<ImageEditorFactory>();
    }
}