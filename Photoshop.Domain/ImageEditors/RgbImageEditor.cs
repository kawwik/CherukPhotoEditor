namespace Photoshop.Domain.ImageEditors;

public class RgbImageEditor : IImageEditor
{
    private ImageData _imageData;
    private ColorSpace _colorSpace;
    private readonly IColorSpaceConverter _colorSpaceConverter;
    
    public RgbImageEditor(ImageData imageData, ColorSpace colorSpace, IColorSpaceConverter colorSpaceConverter)
    {
        if (imageData.PixelFormat is not PixelFormat.Rgb)
            throw new Exception("Картинка должна быть в формате RGB");
        
        _imageData = imageData;
        _colorSpace = colorSpace;
        _colorSpaceConverter = colorSpaceConverter;
    }

    public ImageData GetData() => _imageData;

    public void SetColorSpace(ColorSpace newColorSpace)
    {
        _colorSpace = newColorSpace;
        _imageData = _colorSpaceConverter.Convert(_imageData, _colorSpace, newColorSpace);
    }
}