namespace Photoshop.Domain.ImageEditors;

public interface IDitheringConverter
{
    ImageData Convert(ImageData source, DitheringType ditheringType, int depth);
}