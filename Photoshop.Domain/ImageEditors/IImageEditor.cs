namespace Photoshop.Domain.ImageEditors;

public interface IImageEditor
{
    ImageData GetData();

    ImageData GetRgbData(float gamma, bool[]? channels);

    void SetColorSpace(ColorSpace newColorSpace);

    void ConvertGamma(float gamma);
}