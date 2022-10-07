namespace Photoshop.Domain.ImageEditors.Factory;

public class ImageEditorFactory : IImageEditorFactory
{
    public IImageEditor GetImageEditor(ImageData imageData)
    {
        return imageData.PixelFormat switch
        {
            PixelFormat.Rgb => new RgbImageEditor(imageData),
            PixelFormat.Gray => new GrayImageEditor(imageData),
            _ => throw new NotSupportedException($"Данный формат картинки не поддерживается: {imageData.PixelFormat}")
        };
    }
}