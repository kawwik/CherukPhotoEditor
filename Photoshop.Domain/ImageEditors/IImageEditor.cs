namespace Photoshop.Domain.ImageEditors;

public interface IImageEditor
{
    ImageData GetData();

    ImageData GetDitheredData(DitheringType ditheringType, int ditheringDepth);

    ImageData GetRgbData(float gamma, DitheringType ditheringType, int ditheringDepth, bool[]? channels);

    void SetColorSpace(ColorSpace newColorSpace);

    void SetGamma(float gamma);
}