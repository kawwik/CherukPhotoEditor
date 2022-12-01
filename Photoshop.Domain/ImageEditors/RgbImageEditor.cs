namespace Photoshop.Domain.ImageEditors;

public class RgbImageEditor : IImageEditor
{
    private ImageData _imageData;
    private ColorSpace _colorSpace;
    private float _imageGamma;
    private DitheringType _ditheringType;
    private int _ditheringDepth;
    private readonly IColorSpaceConverter _colorSpaceConverter;
    private readonly IGammaConverter _gammaConverter;
    private readonly IDitheringConverter _ditheringConverter;

    public RgbImageEditor(
        ImageData imageData, 
        ColorSpace colorSpace, 
        float imageGamma,
        DitheringType ditheringType,
        int ditheringDepth,
        IColorSpaceConverter colorSpaceConverter,
        IGammaConverter gammaConverter,
        IDitheringConverter ditheringConverter)
    {
        if (imageData.PixelFormat is not PixelFormat.Rgb)
            throw new Exception("Картинка должна быть в формате RGB");

        if (ditheringDepth < 1 || ditheringDepth > 8)
            throw new ArgumentException("Некорректная глубина дизеринга " + _ditheringDepth);

        _imageData = imageData;
        _colorSpace = colorSpace;
        _imageGamma = imageGamma;
        _ditheringType = ditheringType;
        _ditheringDepth = ditheringDepth;
        _colorSpaceConverter = colorSpaceConverter;
        _gammaConverter = gammaConverter;
        _ditheringConverter = ditheringConverter;
    }

    public ImageData GetData() => _imageData;

    public ImageData GetRgbData(float gamma, bool[]? channels = default) =>
        _ditheringConverter.Convert(_gammaConverter.ConvertGamma(_colorSpaceConverter.ToRgb(_imageData, _colorSpace, channels), _imageGamma, gamma),  _ditheringType, _ditheringDepth);

    public ImageData GetSaveData() =>
        _ditheringConverter.Convert(_imageData, _ditheringType, _ditheringDepth);

        public void ConvertGamma(float gamma)
    {
        _imageData = _gammaConverter.ConvertGamma(_imageData, _imageGamma, gamma);
        _imageGamma = gamma;
    }

    public void SetColorSpace(ColorSpace newColorSpace)
    {
        _imageData = _colorSpaceConverter.Convert(_imageData, _colorSpace, newColorSpace);
        _colorSpace = newColorSpace;
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