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
        await using var stream = File.Open(path, FileMode.Open);
        
        var length = (int)stream.Length;
        var bytes = new byte[length];
        await stream.ReadAsync(bytes, 0, length);

        var image = _imageFactory.GetImage(bytes);

        var imageData = image.GetData();
        return _imageEditorFactory.GetImageEditor(imageData, colorSpace, GammaContext.DefaultGamma);
    }

    public async Task SaveImage(IImageEditor imageEditor, string path)
    {
        if (path.Length < 4)
            throw new ArgumentException("Некорректный путь до файла", nameof(path));

        if (imageEditor is null)
            throw new ArgumentNullException(nameof(imageEditor));

        var extension = path.Split('.').LastOrDefault()?.ToLower();
        var image = extension switch
        {
            "pgm" => new PnmImage(imageEditor.GetData(), PixelFormat.Gray),
            "ppm" => new PnmImage(imageEditor.GetData(), PixelFormat.Rgb),
            _ => throw new ArgumentException("Неверное расширение", nameof(path))
        };

        var imageData = image.GetFile();
        await using var fileStream = File.Open(path, FileMode.Create);
        await fileStream.WriteAsync(imageData);
    }
}