namespace Photoshop.Domain.Images;

public interface IImage
{
    ImageData GetData();

    byte[] GetFile();
}