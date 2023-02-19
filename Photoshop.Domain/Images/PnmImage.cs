using System.Text;
using System.Text.RegularExpressions;
using System;
using System.IO.Compression;
using Photoshop.Domain.Utils.Exceptions;

namespace Photoshop.Domain.Images;

public record PnmImage : IImage
{
    public static bool CheckFileHeader(byte[] image)
    {
        return image[0] == 'P' && (image[1] == '5' || image[1] == '6');
    }
    
    private readonly ImageData _data;

    private ImageData CreateImageData(byte[] pixels, PixelFormat pixelFormat, int height, int width, int maxVal)
    {
        if (pixels.Any(pixel => pixel > maxVal))
        {
            throw new OpenImageException("Изображение не является корректным PNM");
        }
        
        var coefficient = 255.0f / maxVal; 
        var newPixels = Array.ConvertAll(pixels, x => x * coefficient);

        return new ImageData(newPixels, pixelFormat, height, width);
    }

    public PnmImage(byte[] pixels, PixelFormat pixelFormat, int height, int width, int maxVal)
    {
        _data = CreateImageData(pixels, pixelFormat, height, width, maxVal);
    }

    public PnmImage(byte[] image)
    {
        var str = Encoding.Latin1.GetString(image);

        var imageParams = Regex.Match(str, "^(P[5|6])\\s+(\\d+)\\s+(\\d+)\\s+(\\d+)\\s", RegexOptions.Compiled);
        if (!imageParams.Success)
        {
            throw new OpenImageException("Изображение не является PNM");
        }

        var groups = imageParams.Groups;
        var type = groups[1].ToString().Equals("P5") ? PixelFormat.Gray : PixelFormat.Rgb;
        var width = Int32.Parse(groups[2].ToString());
        var height = Int32.Parse(groups[3].ToString());
        var maxValue = Int32.Parse(groups[4].ToString());
        var pixels = new byte[image.Length - imageParams.Length];
        Array.Copy(image, imageParams.Length, pixels, 0, pixels.Length);
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

    public Task<byte[]> GetFileAsync()
    {
        var header = new StringBuilder();
        header.Append(_data.PixelFormat == PixelFormat.Gray ? "P5\n" : "P6\n");
        header.Append(_data.Width + " " +  _data.Height + "\n255\n");
        
        var output = new byte[header.Length + _data.Pixels.Length];
        Encoding.Latin1.GetBytes(header.ToString()).CopyTo(output, 0);
        for (int i = 0; i < _data.Pixels.Length; i++)
        {
            output[header.Length + i] = (byte) _data.Pixels[i];
        }

        byte[] newPixels = Array.ConvertAll(_data.Pixels, x => (byte) (x * 255));

        return Task.FromResult(output);
    }
}