using System;
using System.Reactive;
using System.Threading.Tasks;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.View.Services.Interfaces;
using ReactiveUI;

namespace Photoshop.View;

public class CommandFactory
{
    private readonly IImageService _imageService;
    private readonly IDialogService _dialogService;

    public CommandFactory(IImageService imageService, IDialogService dialogService)
    {
        _imageService = imageService;
        _dialogService = dialogService;
    }
    
    public ReactiveCommand<ColorSpace, IImageEditor?> OpenImage =>
        ReactiveCommand.CreateFromTask<ColorSpace, IImageEditor?>(OpenImageAsync);

    public ReactiveCommand<ImageData, Unit> SaveImage=>
        ReactiveCommand.CreateFromTask<ImageData>(SaveImageAsync);
    
    private async Task<IImageEditor?> OpenImageAsync(ColorSpace colorSpace)
    {
        var path = await _dialogService.ShowOpenFileDialogAsync();
        if (path is null) 
            return null;
        
        return await _imageService.OpenImageAsync(path, colorSpace);
    }

    private async Task SaveImageAsync(ImageData imageData)
    {
        if (imageData is null)
            throw new InvalidOperationException("Нет открытого изображения");
        
        var path = await _dialogService.ShowSaveFileDialogAsync();
        if (path is null) return;
        
        await _imageService.SaveImageAsync(imageData, path);
    }
}