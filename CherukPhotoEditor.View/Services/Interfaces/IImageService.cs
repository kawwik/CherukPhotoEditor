using System.Threading.Tasks;
using CherukPhotoEditor.Domain;
using CherukPhotoEditor.Domain.ImageEditors;

namespace CherukPhotoEditor.View.Services.Interfaces;

public interface IImageService
{
    Task<ImageData> OpenImageAsync(string path, ColorSpace colorSpace);

    Task SaveImageAsync(ImageData imageData, string path);
}