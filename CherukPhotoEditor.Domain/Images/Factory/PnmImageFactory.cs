namespace CherukPhotoEditor.Domain.Images.Factory;

public class PnmImageFactory : IImageFactory
{
    public IImage GetImage(byte[] image)
    {
        return new PnmImage(image);
    }
}