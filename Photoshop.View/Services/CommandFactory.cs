using System;
using System.Reactive;
using System.Threading.Tasks;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.View.Services.Interfaces;
using ReactiveUI;

namespace Photoshop.View.Services;

public class CommandFactory : ICommandFactory
{
    private readonly IImageService _imageService;
    private readonly IDialogService _dialogService;
    private readonly IImageEditorFactory _imageEditorFactory;

    public CommandFactory(IImageService imageService, IDialogService dialogService, IImageEditorFactory imageEditorFactory)
    {
        _imageService = imageService;
        _dialogService = dialogService;
        _imageEditorFactory = imageEditorFactory;
    }
    
    public ReactiveCommand<ColorSpace, IImageEditor?> OpenImage() =>
        ReactiveCommand.CreateFromTask<ColorSpace, IImageEditor?>(OpenImageInternalAsync);

    public ReactiveCommand<ImageData, Unit> SaveImage(IObservable<bool> canExecute) =>
        ReactiveCommand.CreateFromTask<ImageData>(SaveImageInternalAsync, canExecute);

    public ReactiveCommand<Unit, IImageEditor> GenerateGradient() =>
        ReactiveCommand.Create(GenerateGradientInternal);

    private async Task<IImageEditor?> OpenImageInternalAsync(ColorSpace colorSpace)
    {
        var path = await _dialogService.ShowOpenFileDialogAsync();
        if (path is null) 
            return null;
        
        return await _imageService.OpenImageAsync(path, colorSpace);
    }

    private async Task SaveImageInternalAsync(ImageData imageData)
    {
        if (imageData is null)
            throw new InvalidOperationException("Нет открытого изображения");
        
        var path = await _dialogService.ShowSaveFileDialogAsync();
        if (path is null) return;
        
        await _imageService.SaveImageAsync(imageData, path);
    }

    private IImageEditor GenerateGradientInternal()
    {
        int width = 100;
        int height = 100;
        PixelFormat pixelFormat = PixelFormat.Gray;
        
        int coef = pixelFormat == PixelFormat.Rgb ? 3 : 1;
        float[] newPixels = new float[height * width * coef];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int pos = i * width + j;
                for (int col = 0; col < coef; col++)
                {
                    int ind = pos * coef + col;
                    newPixels[ind] = 255 * (1.0f * j / width);
                }
            }
        }
        
        return _imageEditorFactory.GetImageEditor(new ImageData(newPixels, pixelFormat, height, width), ColorSpace.Rgb, 1);
    }
}