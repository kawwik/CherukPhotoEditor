using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.View.Extensions;
using Photoshop.View.Services.Interfaces;
using ReactiveUI;
using IAvaloniaImage = Avalonia.Media.IImage;

namespace Photoshop.View.ViewModels;

public class PhotoEditionContext : ReactiveObject, IDisposable
{
    private readonly IImageService _imageService;
    private readonly IDialogService _dialogService;

    private IImageEditor? _imageEditor;
    
    private readonly List<IDisposable> _subscriptions = new();

    public PhotoEditionContext(
        IDialogService dialogService,
        ColorSpaceContext colorSpaceContext,
        GammaContext gammaContext,
        IImageService imageService)
    {
        _dialogService = dialogService;

        ColorSpaceContext = colorSpaceContext;
        GammaContext = gammaContext;
        _imageService = imageService;

        ImageData = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor),
            ColorSpaceContext.Channels,
            GammaContext.ObservableForPropertyValue(x => x.InnerGamma),
            GammaContext.ObservableForPropertyValue(x => x.OutputGamma),
            (imageEditor, channels, _, outputGamma) => imageEditor?.GetRgbData((float)outputGamma, channels));

        GammaContext.ObservableForPropertyValue(x => x.InnerGamma)
            .Subscribe(x => ImageEditor?.ConvertGamma((float)x))
            .AddTo(_subscriptions);

        ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace)
            .Subscribe(x => ImageEditor?.SetColorSpace(x))
            .AddTo(_subscriptions);
    }

    // TODO: добавить обработку ошибок
    public ReactiveCommand<Unit, Unit> OpenImage => ReactiveCommand.CreateFromTask(async () =>
    {
        var path = await _dialogService.ShowOpenFileDialogAsync();
        ImageEditor = await _imageService.OpenImageAsync(path, ColorSpaceContext.CurrentColorSpace);
    });

    public ReactiveCommand<Unit, Unit> SaveImage => ReactiveCommand.CreateFromTask(async () =>
        {
            var path = await _dialogService.ShowSaveFileDialogAsync();
            await _imageService.SaveImage(ImageEditor, path);
        },
        canExecute: ImageData.Select(x => x is not null));

    public ColorSpaceContext ColorSpaceContext { get; }
    public GammaContext GammaContext { get; }
    public IObservable<ImageData?> ImageData { get; }

    private IImageEditor? ImageEditor
    {
        get => _imageEditor;
        set => _imageEditor = this.RaiseAndSetIfChanged(ref _imageEditor, value);
    }

    private Task OnError(string message) => _dialogService.ShowError(message);

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}