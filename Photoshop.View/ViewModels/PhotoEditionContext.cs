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
    private readonly IImageFactory _imageFactory;
    private readonly IImageEditorFactory _imageEditorFactory;
    private readonly IDialogService _dialogService;

    private IImageEditor? _imageEditor;

    private readonly List<IDisposable> _subscriptions = new();

    public PhotoEditionContext(
        OpenImageCommand openImage,
        SaveImageCommand saveImage,
        IImageFactory imageFactory,
        IImageEditorFactory imageEditorFactory,
        IImageConverter imageConverter,
        IDialogService dialogService,
        ColorSpaceContext colorSpaceContext,
        GammaContext gammaContext)
    {
        _imageFactory = imageFactory;
        _imageEditorFactory = imageEditorFactory;
        _dialogService = dialogService;

        SaveImage = saveImage;
        OpenImage = openImage;
        ColorSpaceContext = colorSpaceContext;
        GammaContext = gammaContext;

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
    
    public ColorSpace ColorSpace { get; set; }

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
        var length = (int)imageStream.Length;
        var bytes = new byte[length];
        await imageStream.ReadAsync(bytes, 0, length);

        IImage image;
        try
        {
            image = _imageFactory.GetImage(bytes);
        }
        catch (Exception e)
        {
            await _dialogService.ShowError(e.Message);
            return;
        }

        var imageData = image.GetData();
        ImageEditor = _imageEditorFactory.GetImageEditor(imageData, ColorSpace, GammaContext.DefaultGamma);
    }

    private async Task OnImageSaving(string imagePath)
    {
        if (imagePath.Length < 4)
        {
            await _dialogService.ShowError("Некорректный путь до файла");
            return;
        }

        if (ImageEditor == null)
        {
            await _dialogService.ShowError("Нет открытого изображения");
            return;
        }

        var extension = imagePath.Split('.').LastOrDefault()?.ToLower();
        IImage image;
        switch (extension)
        {
            case "pgm":
                image = new PnmImage(ImageEditor.GetData(), PixelFormat.Gray);
                break;
            case "ppm":
                image = new PnmImage(ImageEditor.GetData(), PixelFormat.Rgb);
                break;
            default:
                await _dialogService.ShowError("Неверное расширение файла");
                return;
        }

        var imageData = image.GetFile();
        await using var fileStream = File.Open(imagePath, FileMode.Create);
        await fileStream.WriteAsync(imageData);
    }

    private async Task OnError(string message)
    {
        await _dialogService.ShowError(message);
    }

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}