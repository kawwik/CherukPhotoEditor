namespace CherukPhotoEditor.Domain.ImageEditors;

public class RgbImageEditor : IImageEditor
{
    private ImageData _imageData;
    private ColorSpace _colorSpace;
    private readonly IColorSpaceConverter _colorSpaceConverter;
    private readonly IGammaConverter _gammaConverter;
    private readonly IDitheringConverter _ditheringConverter;

    public RgbImageEditor(
        ImageData imageData,
        ColorSpace colorSpace,
        IColorSpaceConverter colorSpaceConverter,
        IGammaConverter gammaConverter,
        IDitheringConverter ditheringConverter)
    {
        if (imageData.PixelFormat is not PixelFormat.Rgb)
            throw new Exception("Картинка должна быть в формате RGB");

        _imageData = imageData;
        _colorSpace = colorSpace;
        _colorSpaceConverter = colorSpaceConverter;
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

    public ImageData GetRgbData(float gamma, DitheringType ditheringType, int ditheringDepth,
        bool[]? channels = default)
    {
        if (ditheringDepth is < 1 or > 8)
            throw new ArgumentException("Некорректная глубина дизеринга");

        var result = _colorSpaceConverter.ToRgb(_imageData, _colorSpace, channels);
        result = _gammaConverter.ConvertGamma(result, gamma);
        result = _ditheringConverter.Convert(result, ditheringType, ditheringDepth);

        return result;
    }

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
        _imageData = _colorSpaceConverter.Convert(_imageData, _colorSpace, newColorSpace);
        _colorSpace = newColorSpace;
    }
}