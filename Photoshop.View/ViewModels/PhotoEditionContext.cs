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

    private readonly ObservableAsPropertyHelper<IImageEditor?> _imageEditor;

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

        OpenImage = ReactiveCommand.CreateFromTask<ColorSpace, IImageEditor?>(OpenImageAsync);
        SaveImage = ReactiveCommand.CreateFromTask<ImageData>(SaveImageAsync);

        _imageEditor = OpenImage.ToProperty(this, x => x.ImageEditor);
        _imageEditor.AddTo(_subscriptions);
        
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

        Observable.Merge(OpenImage.ThrownExceptions, SaveImage.ThrownExceptions)
            .Subscribe(x => OnError(x))
            .AddTo(_subscriptions);
    }

    public ReactiveCommand<ColorSpace, IImageEditor?> OpenImage { get; }

    public ReactiveCommand<ImageData, Unit> SaveImage { get; }

    public ColorSpaceContext ColorSpaceContext { get; }
    public GammaContext GammaContext { get; }
    public IObservable<ImageData?> ImageData { get; }

    private IImageEditor? ImageEditor => _imageEditor.Value;

    private Task OnError(Exception exception)
    {
        return _dialogService.ShowErrorAsync(exception.Message);
    }

    private async Task<IImageEditor?> OpenImageAsync(ColorSpace colorSpace)
    {
        var path = await _dialogService.ShowOpenFileDialogAsync();
        if (path is null) 
            return null;
        
        return await _imageService.OpenImageAsync(path, ColorSpaceContext.CurrentColorSpace);
    }

    private async Task SaveImageAsync(ImageData imageData)
    {
        if (imageData is null)
            throw new InvalidOperationException("Нет открытого изображения");
        
        var path = await _dialogService.ShowSaveFileDialogAsync();
        if (path is null) return;
        
        await _imageService.SaveImageAsync(imageData, path);
    }
    
    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}