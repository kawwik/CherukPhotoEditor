namespace Photoshop.Domain.Images;

public interface IImage
{
    ImageData GetData();

    Task<byte[]> GetFileAsync();
}