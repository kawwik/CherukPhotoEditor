using Avalonia.Controls;
using Ninject.Modules;
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
    }
}