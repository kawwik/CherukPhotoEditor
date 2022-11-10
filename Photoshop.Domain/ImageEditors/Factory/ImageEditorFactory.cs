namespace Photoshop.Domain.ImageEditors.Factory;

public class ImageEditorFactory : IImageEditorFactory
{
    private readonly IColorSpaceConverter _colorSpaceConverter;

    public ImageEditorFactory(IColorSpaceConverter colorSpaceConverter)
    {
        _colorSpaceConverter = colorSpaceConverter;
    }

    public IImageEditor GetImageEditor(ImageData imageData, ColorSpace colorSpace)
    {
        return imageData.PixelFormat switch
        {
            PixelFormat.Rgb => new RgbImageEditor(imageData, colorSpace, _colorSpaceConverter),
            PixelFormat.Gray => new GrayImageEditor(imageData),
            _ => throw new NotSupportedException($"Данный формат картинки не поддерживается: {imageData.PixelFormat}")
        };
    }
}