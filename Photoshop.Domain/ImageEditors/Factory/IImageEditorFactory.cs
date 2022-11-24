namespace Photoshop.Domain.ImageEditors.Factory;

public interface IImageEditorFactory
{
    IImageEditor GetImageEditor(ImageData imageData, ColorSpace colorSpace, float imageGamma, float outputGamma);
}