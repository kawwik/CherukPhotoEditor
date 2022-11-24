namespace Photoshop.Domain.ImageEditors;

public class GrayImageEditor : IImageEditor
{
    private ImageData _imageData;
    private float _imageGamma;
    private float _outputGamma;
    private readonly IGammaConverter _gammaConverter;
    public GrayImageEditor(ImageData imageData, float imageGamma, float outputGamma, IGammaConverter gammaConverter)
    {
        if (imageData.PixelFormat is not PixelFormat.Gray)
            throw new Exception("Картинка должна быть серой");

        _imageData = imageData;
        _imageGamma = imageGamma;
        _outputGamma = outputGamma;
        _gammaConverter = gammaConverter;
    }
    
    public ImageData GetData() => _imageData;
    
    public ImageData GetRgbData(bool[]? channels = default) =>
        _gammaConverter.ConvertGamma(_imageData, _imageGamma, _outputGamma);

    public void ConvertGamma(float gamma)
    {
        _imageData = _gammaConverter.ConvertGamma(_imageData, _imageGamma, gamma);
        _imageGamma = gamma;
    }

    public void AssignGamma(float gamma)
    {
        _outputGamma = gamma;
    }
    public void SetColorSpace(ColorSpace newColorSpace)
    {
        throw new NotSupportedException($"Изменение цветового пространства не поддерживается в {GetType().Name}");
    }
}