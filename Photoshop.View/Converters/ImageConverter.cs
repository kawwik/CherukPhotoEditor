using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Photoshop.Domain;
using AvaloniaPixelFormat = Avalonia.Platform.PixelFormat;
using PixelFormat = Photoshop.Domain.PixelFormat;

namespace Photoshop.View.Converters;

public class ImageConverter : IImageConverter
{
    private static readonly Vector s_defaultDpi = new(96, 96);

    public Bitmap ConvertToBitmap(ImageData imageData)
    {
        if (imageData.PixelFormat == PixelFormat.Gray)
        {
            imageData = imageData.SetPixelFormat(PixelFormat.Rgb);
        }

        var pixels = new byte[imageData.Height * imageData.Width * 4];
        var stride = imageData.Width * 4;

        for (int i = 0; i < imageData.Height * imageData.Width; i++)
        {
            pixels[i * 4] = imageData.Pixels[i * 3];
            pixels[i * 4 + 1] = imageData.Pixels[i * 3 + 1];
            pixels[i * 4 + 2] = imageData.Pixels[i * 3 + 2];
            pixels[i * 4 + 3] = 255;
        }
        
        GCHandle pinnedArray = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();

        var pixelFormat = AvaloniaPixelFormat.Rgba8888;

        return new Bitmap(
            pixelFormat,
            AlphaFormat.Opaque,
            pointer,
            new PixelSize(imageData.Width, imageData.Height),
            s_defaultDpi,
            stride);
    }
}