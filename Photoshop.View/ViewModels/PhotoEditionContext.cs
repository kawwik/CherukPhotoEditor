﻿using System;
using System.IO;
using System.Threading.Tasks;
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

    private IImageEditor? _imageEditor = null;

    public PhotoEditionContext(
        OpenImageCommand openImage, 
        SaveImageCommand saveImage, 
        IImageFactory imageFactory, 
        IImageEditorFactory imageEditorFactory, 
        IImageConverter imageConverter,
        IDialogService dialogService)
    {
        _imageFactory = imageFactory;
        _imageEditorFactory = imageEditorFactory;
        _imageConverter = imageConverter;
        _dialogService = dialogService;

        SaveImage = saveImage;
        OpenImage = openImage;
        
        OpenImage.StreamCallback = OnImageOpening;
        SaveImage.PathCallback = OnImageSaving;
    }

    public OpenImageCommand OpenImage { get; }
    public SaveImageCommand SaveImage { get; }
    
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
            this.RaisePropertyChanged("Image");
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
        ImageEditor = _imageEditorFactory.GetImageEditor(imageData);
    }

    private async Task OnImageSaving(string imagePath)
    {
        if (imagePath.Length < 4)
            await _dialogService.ShowError("Некорректный путь до файла");

        if (ImageEditor == null)
            await _dialogService.ShowError("Нет открытого изображения");

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
}