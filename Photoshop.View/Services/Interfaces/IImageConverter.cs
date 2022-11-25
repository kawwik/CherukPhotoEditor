using Avalonia.Media.Imaging;
using Photoshop.Domain;

namespace Photoshop.View.Services.Interfaces;

public interface IImageConverter
{
    Bitmap ConvertToBitmap(ImageData imageData);
}