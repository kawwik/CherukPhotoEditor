using Avalonia.Controls;
using Ninject.Modules;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.Domain.Images.Factory;
using Photoshop.Domain.Utils;
using Photoshop.View.Services;
using Photoshop.View.Services.Interfaces;
using Photoshop.View.ViewModels;

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
        Bind<IImageEditorFactory>().To<ImageEditorFactory>();
        Bind<IImageFactory>().To<PnmImageFactory>();
        Bind<IColorSpaceConverter>().To<ColorSpaceConverter>();
        Bind<IGammaConverter>().To<GammaConverter>();
        Bind<IImageService>().To<ImageService>();

        Bind<PhotoEditionContext>().To<PhotoEditionContext>().InSingletonScope();
        Bind<ColorSpaceContext>().To<ColorSpaceContext>().InSingletonScope();
    }
}