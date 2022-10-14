namespace Photoshop.Domain.ImageEditors;

public class MinimalImageEditor : IImageEditor
{
    private readonly ImageData _imageData = new ImageData(new byte[3] , PixelFormat.Rgb, 1, 1);

    public ImageData GetData() => _imageData;

}