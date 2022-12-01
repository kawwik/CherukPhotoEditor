namespace Photoshop.Domain.ImageEditors;

public class GrayImageEditor : IImageEditor
{
    private ImageData _imageData;
    private float _imageGamma;
    private DitheringType _ditheringType;
    private int _ditheringDepth;
    private readonly IGammaConverter _gammaConverter;
    private readonly IDitheringConverter _ditheringConverter;
    
    public GrayImageEditor(ImageData imageData, float imageGamma, DitheringType ditheringType, int ditheringDepth, IGammaConverter gammaConverter, IDitheringConverter ditheringConverter)
    {
        if (imageData.PixelFormat is not PixelFormat.Gray)
            throw new Exception("Картинка должна быть серой");

        if (ditheringDepth < 1 || ditheringDepth > 8)
            throw new ArgumentException("Некорректная глубина дизеринга");
        
        _imageData = imageData;
        _imageGamma = imageGamma;
        _ditheringType = ditheringType;
        _ditheringDepth = ditheringDepth;
        _gammaConverter = gammaConverter;
        _ditheringConverter = ditheringConverter;
    }
    
    public ImageData GetData() => _imageData;
    
    public ImageData GetRgbData(float gamma, bool[]? channels = default) =>
        _ditheringConverter.Convert(_gammaConverter.ConvertGamma(_imageData, _imageGamma, gamma), _ditheringType, _ditheringDepth);

    public ImageData GetSaveData() =>
        _ditheringConverter.Convert(_imageData, _ditheringType, _ditheringDepth);

    public void ConvertGamma(float gamma)
    {
        _imageData = _gammaConverter.ConvertGamma(_imageData, _imageGamma, gamma);
        _imageGamma = gamma;
    }
    
    public void SetColorSpace(ColorSpace newColorSpace)
    {
        throw new NotSupportedException($"Изменение цветового пространства не поддерживается в {GetType().Name}");
    }
    
    public void SetDitheringType(DitheringType newType)
    {
        _ditheringType = newType;
    }

    public void SetDitheringDepth(int newDepth)
    {
        if (newDepth < 1 || newDepth > 8)
        {
            throw new ArgumentException("Некорректная глубина дизеринга");
        }

        _ditheringDepth = newDepth;
    }
}