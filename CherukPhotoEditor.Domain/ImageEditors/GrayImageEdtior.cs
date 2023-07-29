namespace CherukPhotoEditor.Domain.ImageEditors;

public class GrayImageEditor : IImageEditor
{
    private ImageData _imageData;
    private readonly IGammaConverter _gammaConverter;
    private readonly IDitheringConverter _ditheringConverter;
    
    public GrayImageEditor(ImageData imageData, IGammaConverter gammaConverter, IDitheringConverter ditheringConverter)
    {
        if (imageData.PixelFormat is not PixelFormat.Gray)
            throw new Exception("Картинка должна быть серой");
        
        _imageData = imageData;
        _gammaConverter = gammaConverter;
        _ditheringConverter = ditheringConverter;
    }
    
    public ImageData GetData() => _imageData;
    public ImageData GetDitheredData(DitheringType ditheringType, int ditheringDepth)
    {
        if (ditheringDepth is < 1 or > 8)
            throw new ArgumentException("Некорректная глубина дизеринга");
        
        return _ditheringConverter.Convert(_imageData, ditheringType, ditheringDepth);
    }

    public ImageData GetRgbData(float gamma, DitheringType ditheringType, int ditheringDepth, bool[]? channels = default)
    {
        var result = _gammaConverter.ConvertGamma(_imageData, gamma);
        result = _ditheringConverter.Convert(result, ditheringType, ditheringDepth);

        return result;
    }

    public ImageData GetSaveData(DitheringType ditheringType, int ditheringDepth) =>
        _ditheringConverter.Convert(_imageData, ditheringType, ditheringDepth);

    public void SetGamma(float gamma)
    {
        _imageData = new ImageData(
            _imageData.Pixels,
            _imageData.PixelFormat,
            _imageData.Height,
            _imageData.Width,
            gamma);
    }

    public void ConvertGamma(float gamma)
    {
        _imageData = _gammaConverter.ConvertGamma(_imageData, gamma);
    }
    
    public void SetColorSpace(ColorSpace newColorSpace)
    {
        throw new NotSupportedException($"Изменение цветового пространства не поддерживается в {GetType().Name}");
    }
}