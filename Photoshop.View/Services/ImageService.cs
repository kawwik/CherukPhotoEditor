using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.Domain.Images;
using Photoshop.Domain.Images.Factory;
using Photoshop.View.Services.Interfaces;
using Photoshop.View.ViewModels;

namespace Photoshop.View.Services;

public class ImageService : IImageService
{
    private readonly IImageFactory _imageFactory;
    private readonly IImageEditorFactory _imageEditorFactory;

    public ImageService(IImageFactory imageFactory, IImageEditorFactory imageEditorFactory)
    {
        _imageFactory = imageFactory;
        _imageEditorFactory = imageEditorFactory;
    }

    public async Task<IImageEditor> OpenImageAsync(string path, ColorSpace colorSpace)
    {
        var bytes = await File.ReadAllBytesAsync(path);

        var image = _imageFactory.GetImage(bytes);

        var imageData = image.GetData();
        return _imageEditorFactory.GetImageEditor(imageData, colorSpace, GammaContext.DefaultGamma);
    }

    public async Task SaveImageAsync(ImageData? imageData, string path)
    {
        if (path.Length < 4)
            throw new ArgumentException("Некорректный путь до файла", nameof(path));

        if (imageData is null)
            throw new ArgumentNullException(nameof(imageData));

        var extension = path.Split('.').LastOrDefault()?.ToLower();
        IImage image = extension switch
        {
            "pgm" => new PnmImage(imageData, PixelFormat.Gray),
            "ppm" => new PnmImage(imageData, PixelFormat.Rgb),
            "png" => new PngImage(imageData),
            _ => throw new ArgumentException("Неверное расширение", nameof(path))
        };
        
        await File.WriteAllBytesAsync(path, await image.GetFileAsync());
    }
}