﻿using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CherukPhotoEditor.Domain;
using PixelFormat = CherukPhotoEditor.Domain.PixelFormat;

namespace CherukPhotoEditor.View.Utils.Converters;

public class ImageDataConverter : ConverterBase<ImageData, Bitmap>
{
    private const Avalonia.Platform.PixelFormat OutputPixelFormat = Avalonia.Platform.PixelFormat.Rgba8888;
    private static readonly Vector s_defaultDpi = new(96, 96);

    protected override Bitmap ConvertInternal(ImageData source)
    {
        var imageData = source;

        if (imageData.PixelFormat is PixelFormat.Gray)
            imageData = imageData.SetPixelFormat(PixelFormat.Rgb);

        var pixels = new byte[imageData.Height * imageData.Width * 4];
        var stride = imageData.Width * 4;

        for (int i = 0; i < imageData.Height * imageData.Width; i++)
        {
            pixels[i * 4] = (byte)imageData.Pixels[i * 3];
            pixels[i * 4 + 1] = (byte)imageData.Pixels[i * 3 + 1];
            pixels[i * 4 + 2] = (byte)imageData.Pixels[i * 3 + 2];
            pixels[i * 4 + 3] = 255;
        }

        var pinnedArray = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        var pointer = pinnedArray.AddrOfPinnedObject();

        return new Bitmap(
            OutputPixelFormat,
            AlphaFormat.Opaque,
            pointer,
            new PixelSize(imageData.Width, imageData.Height),
            s_defaultDpi,
            stride);
    }
}