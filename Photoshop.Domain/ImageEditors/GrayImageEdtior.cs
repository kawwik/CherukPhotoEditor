namespace Photoshop.Domain.ImageEditors;

public class GrayImageEditor : IImageEditor
{
    private readonly ImageData _imageData;
    
    public GrayImageEditor(ImageData imageData)
    {
        if (imageData.PixelFormat is not PixelFormat.Gray)
            throw new Exception("Картинка должна быть серой");

        _imageData = imageData;
    }
    
    public ImageData GetData() => _imageData;
}