namespace Photoshop.Domain.ImageEditors;

public interface IImageEditor
{
    ImageData GetData();

    ImageData GetRgbData(bool[]? channels);

    void SetColorSpace(ColorSpace newColorSpace);
}