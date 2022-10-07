namespace Photoshop.Domain;

public record ImageData()
{
    public byte[] Pixels { get; }
    public PixelFormat PixelFormat { get; }
    public int Height { get; }
    public int Width { get; }
    
    // Не уверен, что конструкторы record работают именно так - подправь, если я ошибся
    public ImageData(byte[] pixels, PixelFormat pixelFormat, int height, int width) : this()
    {
        if (pixels.Length != height * width * (pixelFormat == PixelFormat.Gray ? 1 : 3))
        {
            throw new ArgumentException("Incorrect pixel array size"); 
        }
        this.Pixels = pixels;
        this.PixelFormat = pixelFormat;
        this.Height = height;
        this.Width = width;
    }

    public ImageData SetPixelFormat(PixelFormat newFormat)
    {
        if (PixelFormat == newFormat)
        {
            return this;
        }
        else if (PixelFormat == PixelFormat.Gray && newFormat == PixelFormat.Rgb)
        {
            var newPixels = new byte[Pixels.Length * 3];
            for (var i = 0; i < Pixels.Length; i++)
            {
                newPixels[i * 3] = newPixels[i * 3 + 1] = newPixels[i * 3 + 2] = Pixels[i];
            }

            return new ImageData(newPixels, PixelFormat.Rgb, Height, Width);
        }
        else
        {
            var newPixels = new byte[Pixels.Length / 3];
            for (var i = 0; i < Pixels.Length; i++)
            {
                newPixels[i * 3] = newPixels[i * 3 + 1] = newPixels[i * 3 + 2] = Pixels[i];
            }

            return new ImageData(newPixels, PixelFormat.Rgb, Height, Width);
        }
    }
} 