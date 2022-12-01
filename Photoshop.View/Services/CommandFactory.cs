using System;
using System.Reactive;
using System.Threading.Tasks;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.View.Services.Interfaces;
using ReactiveUI;

namespace Photoshop.View.Services;

public class CommandFactory : ICommandFactory
{
    private readonly IImageService _imageService;
    private readonly IDialogService _dialogService;

    public CommandFactory(IImageService imageService, IDialogService dialogService)
    {
        _imageService = imageService;
        _dialogService = dialogService;
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
        throw new NotImplementedException();
    }
}