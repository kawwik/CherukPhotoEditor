using System.Text;

namespace Photoshop.Domain.Images;

public record PnmImage : IImage
{
    private readonly ImageData _data;

    public PnmImage(byte[] pixels, PixelFormat pixelFormat, int height, int width, int maxVal)
    {
        if (pixels.Any(pixel => pixel > maxVal))
        {
            throw new ArgumentException("Invalid pixel data");
        }

        var newPixels = (byte[]) pixels.Clone();
        var coefficient = (byte) (255 / maxVal); 
        for (var i = 0; i < pixels.Length; i++)
        {
            newPixels[i] *= coefficient;
        }
        
        _data = new ImageData(newPixels, pixelFormat, height, width);
    }

    public PnmImage(ImageData data, PixelFormat newFormat) //MaxVal is set to 255
    {
         _data = data.SetPixelFormat(newFormat);
    }
    
    public ImageData GetData()
    {
        return  _data;
    }

    public byte[] GetFile()
    {
        var header = new StringBuilder();
        header.Append(_data.PixelFormat == PixelFormat.Gray ? "P5" : "P6");
        header.Append(_data.Height + " " +  _data.Width + "\n");

        var output =
            new byte[header.Length +  _data.Pixels.Length];
        Encoding.ASCII.GetBytes(header.ToString()).CopyTo(output, 0);
         _data.Pixels.CopyTo(output, header.Length);

        return output;
    }
}