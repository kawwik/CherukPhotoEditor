using System.Text;
using System.Text.RegularExpressions;
using Photoshop.Domain.Utils.Exceptions;

namespace Photoshop.Domain.Images;

public record PnmImage : IImage
{
    private readonly ImageData _data;

    private ImageData CreateImageData(byte[] pixels, PixelFormat pixelFormat, int height, int width, int maxVal)
    {
        if (pixels.Any(pixel => pixel > maxVal))
        {
            throw new OpenImageException("Изображение не является корректным PNM");
        }

        var newPixels = (byte[]) pixels.Clone();
        var coefficient = (byte) (255 / maxVal); 
        for (var i = 0; i < pixels.Length; i++)
        {
            newPixels[i] *= coefficient;
        }
        
        return new ImageData(newPixels, pixelFormat, height, width);
    }

    public PnmImage(byte[] pixels, PixelFormat pixelFormat, int height, int width, int maxVal)
    {
        _data = CreateImageData(pixels, pixelFormat, height, width, maxVal);
    }

    public PnmImage(byte[] image)
    {
        var str = Encoding.Latin1.GetString(image);
        var imageParams = Regex.Matches(str, "^(P[5|6])\\s+(\\d+)\\s+(\\d+)\\s+(\\d+)\\s+(.+)$");
        if (imageParams.Count == 0)
        {
            throw new OpenImageException("Изображение не является PNM");
        }

        var groups = imageParams[0].Groups;
        var type = groups[1].ToString().Equals("P5") ? PixelFormat.Gray : PixelFormat.Rgb;
        var height = Int32.Parse(groups[2].ToString());
        var width = Int32.Parse(groups[3].ToString());
        var maxValue = Int32.Parse(groups[4].ToString());
        var pixels = Encoding.Latin1.GetBytes(groups[5].ToString());
        _data = CreateImageData(pixels, type, height, width, maxValue);
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
        header.Append(_data.PixelFormat == PixelFormat.Gray ? "P5\n" : "P6\n");
        header.Append(_data.Height + " " +  _data.Width + "\n255\n");
        
        var output =
            new byte[header.Length + _data.Pixels.Length];
        Encoding.Latin1.GetBytes(header.ToString()).CopyTo(output, 0);
         _data.Pixels.CopyTo(output, header.Length);

        return output;
    }
}