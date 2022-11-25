using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.Domain.Images;
using Photoshop.Domain.Images.Factory;
using Photoshop.View.Commands;
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
        OpenImageCommand openImage,
        SaveImageCommand saveImage,
        IImageConverter imageConverter,
        IDialogService dialogService,
        ColorSpaceContext colorSpaceContext,
        GammaContext gammaContext, 
        IImageService imageService)
    {
        _dialogService = dialogService;

        SaveImage = saveImage;
        OpenImage = openImage;
        ColorSpaceContext = colorSpaceContext;
        GammaContext = gammaContext;
        _imageService = imageService;

        OpenImage.StreamCallback = OnImageOpening;
        OpenImage.ErrorCallback = OnError;

        SaveImage.PathCallback = OnImageSaving;
        SaveImage.ErrorCallback = OnError;

        Image = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor),
            ColorSpaceContext.Channels,
            GammaContext.ObservableForPropertyValue(x => x.InnerGamma),
            GammaContext.ObservableForPropertyValue(x => x.OutputGamma),
            (imageEditor, channels, _, outputGamma) =>
                imageEditor == null
                    ? null
                    : imageConverter.ConvertToBitmap(imageEditor.GetRgbData((float)outputGamma, channels)));
        
        GammaContext.ObservableForPropertyValue(x => x.InnerGamma)
            .Subscribe(x => ImageEditor?.ConvertGamma((float)x))
            .AddTo(_subscriptions);

        ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace)
            .Subscribe(x => ImageEditor?.SetColorSpace(x))
            .AddTo(_subscriptions);
    }

    public OpenImageCommand OpenImage { get; }
    public SaveImageCommand SaveImage { get; }
    public ColorSpaceContext ColorSpaceContext { get; }
    public GammaContext GammaContext { get; }
    public IObservable<IAvaloniaImage?> Image { get; }

    private IImageEditor? ImageEditor
    {
        get => _imageEditor;
        set
        {
            _imageEditor = this.RaiseAndSetIfChanged(ref _imageEditor, value);
            SaveImage.OnCanExecuteChanged();
        }
    }

    private async Task OnImageOpening(Stream imageStream)
    {
        ImageEditor = await _imageService.OpenImageAsync(imageStream, ColorSpaceContext.CurrentColorSpace);
    }

    private Task OnImageSaving(string imagePath) => _imageService.SaveImage(ImageEditor, imagePath);

    private Task OnError(string message) => _dialogService.ShowError(message);

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}