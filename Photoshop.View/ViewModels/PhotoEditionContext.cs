using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.Domain.Images;
using Photoshop.Domain.Images.Factory;
using Photoshop.View.Commands;
using Photoshop.View.Converters;
using Photoshop.View.Services.Interfaces;
using ReactiveUI;
using IAvaloniaImage = Avalonia.Media.IImage;

namespace Photoshop.View.ViewModels;

public class PhotoEditionContext : ReactiveObject
{
    private readonly IImageFactory _imageFactory;
    private readonly IImageEditorFactory _imageEditorFactory;
    private readonly IImageConverter _imageConverter;
    private readonly IDialogService _dialogService;

    private IImageEditor? _imageEditor;

    public PhotoEditionContext(
        OpenImageCommand openImage, 
        SaveImageCommand saveImage, 
        IImageFactory imageFactory, 
        IImageEditorFactory imageEditorFactory, 
        IImageConverter imageConverter,
        IDialogService dialogService,
        ColorSpaceContext colorSpaceContext)
    {
        _imageFactory = imageFactory;
        _imageEditorFactory = imageEditorFactory;
        _imageConverter = imageConverter;
        _dialogService = dialogService;

        SaveImage = saveImage;
        OpenImage = openImage;
        ColorSpaceContext = colorSpaceContext;
        
        OpenImage.StreamCallback = OnImageOpening;
        OpenImage.ErrorCallback = OnError;
        
        SaveImage.PathCallback = OnImageSaving;
        SaveImage.ErrorCallback = OnError;
    }

    public OpenImageCommand OpenImage { get; }
    public SaveImageCommand SaveImage { get; }
    public ColorSpaceContext ColorSpaceContext { get; }

    public ColorSpace ColorSpace { get; set; }

    public bool[] Channels { get; } = { true, true, true }; 

    public IAvaloniaImage? Image
    {
        get => ImageEditor == null ? null : _imageConverter.ConvertToBitmap(ImageEditor.GetData());
    }

    private IImageEditor? ImageEditor
    {
        get => _imageEditor;
        set
        {
            _imageEditor = this.RaiseAndSetIfChanged(ref _imageEditor, value);
            this.RaisePropertyChanged(nameof(Image));
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
        ImageEditor = _imageEditorFactory.GetImageEditor(imageData, ColorSpace);
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

        string extension = imagePath.Substring(imagePath.Length - 4, 4).ToLower();
        IImage image;
        switch (extension)
        {
            case ".pgm":
                image = new PnmImage(ImageEditor.GetData(), PixelFormat.Gray);
                break;
            case ".ppm":
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

    private Task OnColorSpaceChanged()
    {
        if (_imageEditor is null)
            return Task.CompletedTask;

        var task = Task.Run(() => _imageEditor.SetColorSpace(ColorSpace));
        task.ContinueWith(_ => this.RaisePropertyChanged(nameof(Image)));

        return task;
    }

    private async Task OnError(string message)
    {
        await _dialogService.ShowError(message);
    }
}