namespace Photoshop.Domain.ImageEditors;

public class RgbImageEditor : IImageEditor
{
    private ImageData _imageData;
    private ColorSpace _colorSpace;
    private float _imageGamma = 1;
    private float _outputGamma = 1;
    private readonly IColorSpaceConverter _colorSpaceConverter;
    private readonly IGammaConverter _gammaConverter;

    public RgbImageEditor(ImageData imageData, ColorSpace colorSpace, float imageGamma, float outputGamma, IColorSpaceConverter colorSpaceConverter, IGammaConverter gammaConverter)
    {
        if (imageData.PixelFormat is not PixelFormat.Rgb)
            throw new Exception("Картинка должна быть в формате RGB");

        _imageData = imageData;
        _colorSpace = colorSpace;
        _imageGamma = imageGamma;
        _outputGamma = outputGamma;
        _colorSpaceConverter = colorSpaceConverter;
        _gammaConverter = gammaConverter;
    }

    public ImageData GetData() => _imageData;

    public ImageData GetRgbData(bool[]? channels = default) =>
        _gammaConverter.ConvertGamma(_colorSpaceConverter.ToRgb(_imageData, _colorSpace, channels), _imageGamma, _outputGamma);

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
        _imageData = _colorSpaceConverter.Convert(_imageData, _colorSpace, newColorSpace);
        _colorSpace = newColorSpace;
    }
}