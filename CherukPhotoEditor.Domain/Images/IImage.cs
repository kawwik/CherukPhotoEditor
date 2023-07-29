namespace CherukPhotoEditor.Domain.Images;

public interface IImage
{
    ImageData GetData();

    Task<byte[]> GetFileAsync();
}