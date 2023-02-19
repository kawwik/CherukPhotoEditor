namespace Photoshop.Domain.Images.Factory;

public class PnmPngImageFactory : IImageFactory
{
    public IImage GetImage(byte[] image)
    {
        if (PnmImage.CheckFileHeader(image))
        {
            return new PnmImage(image);    
        }
        else
        {
            return new PngImage(image);
        }
    }
}