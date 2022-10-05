namespace Photoshop.Domain.Images.Factory;

public interface IImageFactory
{
    IImage GetImage(byte[] image);
}