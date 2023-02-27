namespace Photoshop.Domain;

public interface IGammaConverter
{
    ImageData ConvertGamma(ImageData source, float newGamma);
}