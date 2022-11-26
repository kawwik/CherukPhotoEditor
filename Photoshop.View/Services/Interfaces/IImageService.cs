using System.IO;
using System.Threading.Tasks;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;

namespace Photoshop.View.Services.Interfaces;

public interface IImageService
{
    Task<IImageEditor> OpenImageAsync(string path, ColorSpace colorSpace);

    Task SaveImageAsync(IImageEditor imageEditor, string path);
}