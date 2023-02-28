namespace Photoshop.Domain;

public class GammaConverter : IGammaConverter
{
    private float ConvertValue(float value,  float oldGamma, float newGamma)
    {
        return (float) Math.Pow(value / 256,  newGamma / oldGamma) * 256;
    }
    
    public ImageData ConvertGamma(ImageData source, float newGamma)
    {
        if (Math.Abs(source.Gamma - newGamma) < 0.001)
            return source;
        
        var newPixels = Array.ConvertAll(source.Pixels, x => ConvertValue(x, source.Gamma, newGamma));

        return new ImageData(newPixels, source.PixelFormat, source.Height, source.Width, newGamma);
    }
}