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
        GCHandle pinnedArray = GCHandle.Alloc(imageData.Pixels, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();

        var (pixelFormat, bytePerPixel) = imageData.PixelFormat switch
        {
            PixelFormat.Rgb => (AvaloniaPixelFormat.Rgb565, 3),
            PixelFormat.Gray => (AvaloniaPixelFormat.Bgra8888, 1),
            _ => throw new NotSupportedException("Данный формат пикселей не поддерживается")
        };

        var stride = imageData.Width * bytePerPixel;

        return new Bitmap(
            pixelFormat,
            AlphaFormat.Opaque,
            pointer,
            new PixelSize(imageData.Width, imageData.Height),
            s_defaultDpi,
            stride);
    }
}