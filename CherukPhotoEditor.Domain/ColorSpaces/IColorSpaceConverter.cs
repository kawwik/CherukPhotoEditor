namespace CherukPhotoEditor.Domain;

public interface IColorSpaceConverter
{
    ImageData ToRgb(ImageData source, ColorSpace colorSpace, bool[]? channels = default);

    ImageData FromRgb(ImageData source, ColorSpace newColorSpace);

    ImageData Convert(ImageData source, ColorSpace currentColorSpace, ColorSpace newColorSpace);
}