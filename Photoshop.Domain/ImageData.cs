namespace Photoshop.Domain;

public class ImageData
{
    public float[] Pixels { get; }
    public PixelFormat PixelFormat { get; }
    public int Height { get; }
    public int Width { get; }
    public double Gamma { get; }
    
    public ImageData(float[] pixels, PixelFormat pixelFormat, int height, int width, double gamma)
    {
        if (height <= 0 || width <= 0)
        {
            throw new ArgumentException("Incorrect height or width");
        }

        if (pixels.Length != height * width * (pixelFormat == PixelFormat.Gray ? 1 : 3))
        {
            throw new ArgumentException("Incorrect pixel array size"); 
        }

        Pixels = pixels;
        PixelFormat = pixelFormat;
        Height = height;
        Width = width;
        Gamma = gamma;
    }

    public ImageData SetPixelFormat(PixelFormat newFormat)
    {
        if (PixelFormat == newFormat)
        {
            return this;
        }
        else if (PixelFormat == PixelFormat.Gray && newFormat == PixelFormat.Rgb)
        {
            var newPixels = new float[Pixels.Length * 3];
            for (var i = 0; i < Pixels.Length; i++)
            {
                newPixels[i * 3] = newPixels[i * 3 + 1] = newPixels[i * 3 + 2] = Pixels[i];
            }

            return new ImageData(newPixels, PixelFormat.Rgb, Height, Width, Gamma);
        }
        else
        {
            var newPixels = new float[Pixels.Length / 3];
            for (var i = 0; i < newPixels.Length; i++)
            {
                newPixels[i] = (Pixels[i * 3] + Pixels[i * 3 + 1] + Pixels[i * 3 + 2]) / 3.0f;
            }

            return new ImageData(newPixels, PixelFormat.Gray, Height, Width, Gamma);
        }
    }
} 