namespace Photoshop.Domain.ImageEditors;

public interface IImageEditor
{
    ImageData GetData();

    ImageData GetRgbData(float gamma, bool[]? channels);
    
    ImageData GetSaveData();

    void SetColorSpace(ColorSpace newColorSpace);

    void ConvertGamma(float gamma);

    void SetDitheringType(DitheringType newType);

    void SetDitheringDepth(int newDepth);
}