using System.Text;
using System.Text.RegularExpressions;

namespace Photoshop.Domain.Images.Factory;

public class PnmImageFactory : IImageFactory
{
    public IImage GetImage(byte[] image)
    {
        var str = Encoding.Latin1.GetString(image);
        var imageParams = Regex.Matches(str, "^(P[5|6])\\s+(\\d+)\\s+(\\d+)\\s+(\\d+)\\s+(.+)$");
        if (imageParams.Count == 0)
        {
            throw new ArgumentException("Invalid file data");
        }

        var groups = imageParams[0].Groups;
        var type = groups[1].ToString().Equals("P5") ? PixelFormat.Gray : PixelFormat.Rgb;
        var height = Int32.Parse(groups[2].ToString());
        var width = Int32.Parse(groups[3].ToString());
        var maxValue = Int32.Parse(groups[4].ToString());
        var pixels = Encoding.Latin1.GetBytes(groups[5].ToString());
        Console.WriteLine(groups[5].ToString());
        Console.WriteLine(height);
        Console.WriteLine(width);
        Console.WriteLine(maxValue);
        Console.WriteLine(pixels.Length);
        return new PnmImage(pixels, type, height, width, maxValue);
    }
}