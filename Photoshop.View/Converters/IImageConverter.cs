using Avalonia.Media.Imaging;
using Photoshop.Domain;

namespace Photoshop.View.Converters;

public interface IImageConverter
{
    Bitmap ConvertToBitmap(ImageData imageData);
}