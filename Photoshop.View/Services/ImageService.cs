using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Photoshop.Domain;
using Photoshop.Domain.Images;
using Photoshop.Domain.Images.Factory;
using Photoshop.View.Services.Interfaces;

namespace Photoshop.View.Services;

public class ImageService : IImageService
{
    private readonly IImageFactory _imageFactory;

    public ImageService(IImageFactory imageFactory)
    {
        _imageFactory = imageFactory;
    }

    public async Task<ImageData> OpenImageAsync(string path, ColorSpace colorSpace)
    {
        var bytes = await File.ReadAllBytesAsync(path);

        var image = _imageFactory.GetImage(bytes);

        return image.GetData();

        //return _imageEditorFactory.GetImageEditor(imageData, colorSpace, GammaContext.DefaultGamma);
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