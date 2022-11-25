namespace Photoshop.Domain.ImageEditors;

public class GrayImageEditor : IImageEditor
{
    private ImageData _imageData;
    private float _imageGamma;
    private readonly IGammaConverter _gammaConverter;
    public GrayImageEditor(ImageData imageData, float imageGamma, IGammaConverter gammaConverter)
    {
        if (imageData.PixelFormat is not PixelFormat.Gray)
            throw new Exception("Картинка должна быть серой");

        _imageData = imageData;
        _imageGamma = imageGamma;
        _gammaConverter = gammaConverter;
    }
    
    public ImageData GetData() => _imageData;
    
    public ImageData GetRgbData(float gamma, bool[]? channels = default) =>
        _gammaConverter.ConvertGamma(_imageData, _imageGamma, gamma);

    public void ConvertGamma(float gamma)
    {
        _imageData = _gammaConverter.ConvertGamma(_imageData, _imageGamma, gamma);
        _imageGamma = gamma;
    }
    
    public void SetColorSpace(ColorSpace newColorSpace)
    {
        throw new NotSupportedException($"Изменение цветового пространства не поддерживается в {GetType().Name}");
    }
}