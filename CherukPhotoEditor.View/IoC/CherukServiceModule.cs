using Avalonia.Controls;
using Ninject.Modules;
using CherukPhotoEditor.Domain;
using CherukPhotoEditor.Domain.ImageEditors;
using CherukPhotoEditor.Domain.ImageEditors.Factory;
using CherukPhotoEditor.Domain.Images.Factory;
using CherukPhotoEditor.Domain.Utils;
using CherukPhotoEditor.View.Services;
using CherukPhotoEditor.View.Services.Interfaces;
using CherukPhotoEditor.View.ViewModels;

namespace CherukPhotoEditor.View.IoC;

public class CherukServiceModule : NinjectModule
{
    private readonly Window _mainWindow;

    public CherukServiceModule(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public override void Load()
    {
        Bind<IDialogService>().To<DialogService>().WithConstructorArgument("parentWindow", _mainWindow);
        Bind<IImageEditorFactory>().To<ImageEditorFactory>();
        Bind<IImageFactory>().To<ImageFactory>();
        Bind<IColorSpaceConverter>().To<ColorSpaceConverter>();
        Bind<IGammaConverter>().To<GammaConverter>();
        Bind<IImageService>().To<ImageService>();
        Bind<ICommandFactory>().To<CommandFactory>();
        Bind<IDitheringConverter>().To<DitheringConverter>();

        Bind<PhotoEditionContext>().To<PhotoEditionContext>().InSingletonScope();
        Bind<ColorSpaceContext>().To<ColorSpaceContext>().InSingletonScope();
    }
}