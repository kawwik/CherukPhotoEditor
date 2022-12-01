using System.Net.Sockets;
using System.Security.Cryptography;
using Avalonia.Controls;
using Photoshop.Domain.ImageEditors;

namespace Photoshop.Domain;

public class DitheringConverter : IDitheringConverter
{
    private int GetLevel(float value, int depth, float add=0)
    {
        return (int) Math.Min(value / (1 << (8 - depth)) + add, (1 << depth) - 1);
    }

    private float GetValue(int level, int depth)
    {
        return level * (255.0f / ((1 << depth) - 1));
    }
 
    private ImageData CompressImage(ImageData source, int depth)
    {
        if (depth == 8)
        {
            return new ImageData((float[]) source.Pixels.Clone(), source.PixelFormat, source.Height, source.Width); 
        }
        
        float[] pixels = source.Pixels;
        float[] newPixels = (float[]) pixels.Clone();

        for (int i = 0; i < pixels.Length; i++)
        {
            int level = GetLevel(newPixels[i], depth);
            newPixels[i] = GetValue(level, depth);
        }
        
        return new ImageData(newPixels, source.PixelFormat, source.Height, source.Width);
    }
    
    private ImageData OrderedDithering(ImageData source, int depth)
    {
        float[,] threshold = {{0, 32, 8, 40, 2, 34, 10, 42},
            {48, 16, 56, 24, 50, 18, 58, 26},
            {12, 44, 4, 36, 14, 46, 6, 38},
            {60, 28, 52, 20, 62, 30, 54, 22},
            {3, 35, 11, 43, 1, 33, 9, 41},
            {51, 19, 59, 27, 49, 17, 57, 25},
            {15, 47, 7, 39, 13, 45, 5, 27},
            {63, 31, 55, 23, 61, 29, 53, 21}};
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                threshold[i, j] = (threshold[i, j] + 1) / 64;
            }
        }

        int h = source.Height;
        int w = source.Width;
        float[] pixels = source.Pixels;
        float[] newPixels = (float[]) pixels.Clone();

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                int pos = i * w + j;
                int coef = source.PixelFormat == PixelFormat.Rgb ? 3 : 1;
                for (int col = 0; col < coef; col++)
                {
                    int ind = pos * coef + col;
                    int level = GetLevel(newPixels[ind], depth, (threshold[i % 8, j % 8]) - 0.5f);
                    newPixels[ind] = GetValue(level, depth);
                }
            }
        }
        
        return new ImageData(newPixels, source.PixelFormat, h, w);
    }
    
    private ImageData RandomDithering(ImageData source, int depth)
    {
        float[] pixels = source.Pixels;
        float[] newPixels = (float[]) pixels.Clone();
        var random = new Random();
        
        for (int i = 0; i < pixels.Length; i++)
        {
            int level = GetLevel(newPixels[i], depth, (float) random.NextDouble() - 0.5f);
            newPixels[i] = GetValue(level, depth);
        }
        
        return new ImageData(newPixels, source.PixelFormat, source.Height, source.Width);
    }

    private ImageData FloydSteinbergDithering(ImageData source, int depth)
    {
        int h = source.Height;
        int w = source.Width;
        float[] pixels = source.Pixels;
        float[] newPixels = (float[]) pixels.Clone();
        var random = new Random();
        
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                int pos = i * w + j;
                int coef = source.PixelFormat == PixelFormat.Rgb ? 3 : 1;
                for (int col = 0; col < coef; col++)
                {
                    int ind = pos * coef + col;
                    int level = GetLevel(newPixels[ind], depth, (float) random.NextDouble() - 0.5f);
                    float value =  GetValue(level, depth);
                    float error = newPixels[ind] - value;
                    newPixels[ind] = value;
                    if (j != w - 1)
                    {
                        newPixels[(pos + 1) * coef + col] += error * 7.0f / 16;
                    }
                    if (i != h - 1 && j != 0)
                    {
                        newPixels[((i + 1) * w + j - 1) * coef + col] += error * 3.0f / 16;
                    }
                    if (i != h - 1)
                    {
                        newPixels[((i + 1) * w + j) * coef + col] += error * 5.0f / 16;
                    }
                    if (i != h - 1 && j != w - 1)
                    {
                        newPixels[((i + 1) * w + j + 1) * coef + col] += error * 1.0f / 16;
                    }
                }
            }
        }
        
        return new ImageData(newPixels, source.PixelFormat, h, w);
    }
    
    private ImageData AtkinsonDithering(ImageData source, int depth)
    {
        int h = source.Height;
        int w = source.Width;
        float[] pixels = source.Pixels;
        float[] newPixels = (float[]) pixels.Clone();
        var random = new Random();
        
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                int pos = i * w + j;
                int coef = source.PixelFormat == PixelFormat.Rgb ? 3 : 1;
                for (int col = 0; col < coef; col++)
                {
                    int ind = pos * coef + col;
                    int level = GetLevel(newPixels[ind], depth, (float) random.NextDouble() - 0.5f);
                    float value =  GetValue(level, depth);
                    float error = newPixels[ind] - value;
                    newPixels[ind] = value;
                    if (j != w - 1)
                    {
                        newPixels[(pos + 1) * coef + col] += error * 1.0f / 8;
                    }
                    if (j < w - 2)
                    {
                        newPixels[(pos + 2) * coef + col] += error * 1.0f / 8;
                    }
                    if (i != h - 1 && j != 0)
                    {
                        newPixels[((i + 1) * w + j - 1) * coef + col] += error * 1.0f / 16;
                    }
                    if (i != h - 1)
                    {
                        newPixels[((i + 1) * w + j) * coef + col] += error * 1.0f / 16;
                    }
                    if (i != h - 1 && j != w - 1)
                    {
                        newPixels[((i + 1) * w + j + 1) * coef + col] += error * 1.0f / 8;
                    }
                    if (i < h - 2)
                    {
                        newPixels[((i + 2) * w + j) * coef + col] += error * 1.0f / 8;
                    }
                }
            }
        }
        
        return new ImageData(newPixels, source.PixelFormat, h, w);
    }
    
    public ImageData Convert(ImageData source, DitheringType ditheringType, int depth)
    {
        if (depth < 1 || depth > 8)
            throw new ArgumentException("Некорректная глубина дизеринга");
        
        switch (ditheringType)
        {
            case DitheringType.None:
                return CompressImage(source, depth);
            case DitheringType.Ordered:
                return OrderedDithering(source, depth);
            case DitheringType.Random:
                return RandomDithering(source, depth);
            case DitheringType.FloydSteinberg:
                return FloydSteinbergDithering(source, depth);
            case DitheringType.Atkinson:
                return AtkinsonDithering(source, depth);
            default:
                throw new ArgumentException("Данный тип дизеринга не поддерживается");
        }
    }
}