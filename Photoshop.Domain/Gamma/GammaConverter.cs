namespace Photoshop.Domain;

public class GammaConverter : IGammaConverter
{
    private float ConvertValue(float value,  float oldGamma, float newGamma)
    {
        return (float) Math.Pow(value / 256,  newGamma / oldGamma) * 256;
    }
    
    public ImageData ConvertGamma(ImageData source, float oldGamma, float newGamma)
    {
        var newPixels = Array.ConvertAll(source.Pixels, x => ConvertValue(x, oldGamma, newGamma));

        return new ImageData(newPixels, source.PixelFormat, source.Height, source.Width, newGamma);
    }
}