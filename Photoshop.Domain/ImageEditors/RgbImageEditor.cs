namespace Photoshop.Domain.ImageEditors;

public class RgbImageEditor : IImageEditor
{
    private readonly ImageData _imageData;
    
    public RgbImageEditor(ImageData imageData)
    {
        if (imageData.PixelFormat is not PixelFormat.Rgb)
            throw new Exception("Картинка должна быть в формате RGB");
        
        _imageData = imageData;
    }
    
    public ImageData GetData() => _imageData;
}