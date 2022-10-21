namespace Photoshop.Domain.ImageEditors;

public interface IImageEditor
{
    ImageData GetData();

    void SetColorSpace(ColorSpace newColorSpace);
}