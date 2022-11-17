namespace Photoshop.Domain.Utils;

public class ColorSpaceConverter : IColorSpaceConverter
{
    // H обычно содержит значения от 0 до 360, но в данном случае их нужно нормировать до максимум 255
    private float[] HslFromRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float r2 = pixels[i * 3] / 255.0f;
            float g2 = pixels[i * 3 + 1] / 255.0f;
            float b2 = pixels[i * 3 + 2] / 255.0f;
            float cMax = System.Math.Max(System.Math.Max(r2, g2), b2);
            float cMin = System.Math.Min(System.Math.Min(r2, g2), b2);
            float delta = cMax - cMin;

            if (delta == 0)
            {
                pixels[i * 3] = 0; // Hue
                pixels[i * 3 + 1] = 0; // Saturation
                pixels[i * 3 + 2] = cMax; // Lightness
            }
            else
            {
                // Hue
                const float coef = 60.0f * (255.0f / 360.0f); // Для нормировки h
                if (r2 == cMax)
                    pixels[i * 3] = coef * (((g2 - b2) / delta + 6) % 6.0f);
                else if (g2 == cMax)
                    pixels[i * 3] = coef * ((b2 - r2) / delta + 2.0f);
                else
                    pixels[i * 3] = coef * ((r2 - g2) / delta + 4.0f);

                // Lightness - нужно нормировать до максимум 255
                pixels[i * 3 + 2] = 0.5f * (cMax + cMin) * 255.0f;
                
                // Saturation - нужно нормировать до максимум 255
                pixels[i * 3 + 1] = delta / (1 - Math.Abs(1 - cMax - cMin)) * 255.0f;
            }
        }

        return pixels;
    }

    private float[] HslToRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float coef = 360.0f / 255.0f;
            float h = pixels[i * 3] * coef;
            float s = pixels[i * 3 + 1] / 255.0f;
            float l = pixels[i * 3 + 2] / 255.0f;

            float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
            float p = 2 * l - q;
            float h_k = h / 360.0f;
            float[] t = new float[3];
            t[0] = (h_k + 1.0f / 3 + 1) % 1 ;
            t[1] = (h_k + 1) % 1;
            t[2] = (h_k - 1.0f / 3 + 1) % 1;

            float[] rgb = new float[3];

            for (int j = 0; j < 3; j++)
            { 
                if (t[j] < 1.0f / 6)
                {
                    rgb[j] = p + ((q - p) * 6 * t[j]);
                }
                else if (t[j] < 0.5f)
                {
                    rgb[j] = q;
                }
                else if (t[j] <= 2.0f / 3)
                {
                    rgb[j] = p + ((q - p) * (2.0f / 3 - t[j]) * 6);
                }
                else
                {
                    rgb[j] = p;
                }
            }
            
            pixels[i * 3] = rgb[0] * 255; // r
            pixels[i * 3 + 1] = rgb[1] * 255; // g
            pixels[i * 3 + 2] = rgb[2] * 255; // b
        }

        return pixels;
    }

// H обычно содержит значения от 0 до 360, но в данном случае их нужно нормировать до максимум 255
    private float[] HsvFromRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float r2 = pixels[i * 3] / 255.0f;
            float g2 = pixels[i * 3 + 1] / 255.0f;
            float b2 = pixels[i * 3 + 2] / 255.0f;
            float cMax = System.Math.Max(System.Math.Max(r2, g2), b2);
            float cMin = System.Math.Min(System.Math.Min(r2, g2), b2);
            float delta = cMax - cMin;

            if (delta == 0)
            {
                pixels[i * 3] = 0; // Hue
                pixels[i * 3 + 1] = 0; // Saturation
                pixels[i * 3 + 2] = cMax; // Lightness
            }
            else
            {
                // Hue
                const float coef = 60.0f * (255.0f / 360.0f); // Для нормировки h
                if (r2 == cMax)
                    pixels[i * 3] = coef * (((g2 - b2) / delta + 6.0f) % 6.0f);
                else if (g2 == cMax)
                    pixels[i * 3] = coef * ((b2 - r2) / delta + 2.0f);
                else
                    pixels[i * 3] = coef * ((r2 - g2) / delta + 4.0f);

                // Saturation - нужно нормировать до максимум 255
                pixels[i * 3 + 1] = cMax == 0 ? 0 : delta / cMax * 255.0f;

                // Value - нужно нормировать до максимум 255
                pixels[i * 3 + 2] = cMax * 255.0f;
            }
        }

        return pixels;
    }

    // H обычно содержит значения от 0 до 360, но в данном случае их нужно нормировать до максимум 255
    private float[] HsvToRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float coef = 360.0f / 255.0f;
            float h = pixels[i * 3] * coef;
            float s = pixels[i * 3 + 1] / 255.0f;
            float v = pixels[i * 3 + 2] / 255.0f;

            float c = v * s;
            float x = c * (1 - Math.Abs(h / 60.0f % 2 - 1));
            float m = v - c;

            float r2, g2, b2;
            if (h < 60)
            {
                r2 = c;
                g2 = x;
                b2 = 0;
            }
            else if (h < 120)
            {
                r2 = x;
                g2 = c;
                b2 = 0;
            }
            else if (h < 180)
            {
                r2 = 0;
                g2 = c;
                b2 = x;
            }
            else if (h < 240)
            {
                r2 = 0;
                g2 = x;
                b2 = c;
            }
            else if (h < 300)
            {
                r2 = x;
                g2 = 0;
                b2 = c;
            }
            else
            {
                r2 = c;
                g2 = 0;
                b2 = x;
            }

            pixels[i * 3] = (r2 + m) * 255.0f; // r
            pixels[i * 3 + 1] = (g2 + m) * 255.0f; // g
            pixels[i * 3 + 2] = (b2 + m) * 255.0f; // b
        }

        return pixels;
    }

    private float[] YCbCr601FromRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float r = pixels[i * 3] / 256.0f;
            float g = pixels[i * 3 + 1] / 256.0f;
            float b = pixels[i * 3 + 2] / 256.0f;

            float y = 16 + 65.738f * r + 129.057f * g + 25.064f * b;
            float cb = 128 - 37.797f * r - 74.203f * g + 112 * b;
            float cr = 128 + 112 * r - 93.786f * g - 18.214f * b;

            pixels[i * 3] = y; // Y
            pixels[i * 3 + 1] = cb; // Cb
            pixels[i * 3 + 2] = cr; // Cr
        }

        return pixels;
    }

    private float[] YCbCr601ToRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float y = pixels[i * 3] / 256.0f;
            float cb = pixels[i * 3 + 1] / 256.0f;
            float cr = pixels[i * 3 + 2] / 256.0f;

            float r = 298.082f * y + 408.583f * cr - 222.921f;
            float g = 298.082f * y - 100.291f * cb - 208.120f * cr + 135.576f;
            float b = 298.082f * y + 516.412f * cb - 276.836f;

            pixels[i * 3] = r; // r
            pixels[i * 3 + 1] = g; // g
            pixels[i * 3 + 2] = b; // b
        }

        return pixels;
    }
    
    private float[] YCbCr709FromRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float r = pixels[i * 3];
            float g = pixels[i * 3 + 1];
            float b = pixels[i * 3 + 2];

            float y = 0.299f * r + 0.587f * g + 0.114f * b;
            float cb = 128 - 0.168736f * r - 0.331264f * g + 0.5f * b;
            float cr = 128 + 0.5f * r - 0.418688f * g - 0.081312f * b;

            pixels[i * 3] = y; // Y
            pixels[i * 3 + 1] = cb; // Cb
            pixels[i * 3 + 2] = cr; // Cr
        }

        return pixels;
    }

    private float[] YCbCr709ToRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float y = pixels[i * 3];
            float cb = pixels[i * 3 + 1];
            float cr = pixels[i * 3 + 2];

            float r = y + 1.402f * (cr - 128);
            float g = y - 0.34414f * (cb - 128) - 0.71414f * (cr - 128);
            float b = y + 1.772f * (cb - 128);

            pixels[i * 3] = r; // r
            pixels[i * 3 + 1] = g; // g
            pixels[i * 3 + 2] = b; // b
        }

        return pixels;
    }
    
    private float[] YCbCoFromRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float r = pixels[i * 3];
            float g = pixels[i * 3 + 1];
            float b = pixels[i * 3 + 2];

            float y = 0.25f * r + 0.5f * g + 0.25f * b;
            float cb = 0.5f * r - 0.5f * b;
            float co = -0.25f * r + 0.5f * g - 0.25f * b;

            pixels[i * 3] = y; // Y
            pixels[i * 3 + 1] = cb; // Cb
            pixels[i * 3 + 2] = co; // Co
        }

        return pixels;
    }

    private float[] YCbCoToRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float y = pixels[i * 3];
            float cb = pixels[i * 3 + 1];
            float co = pixels[i * 3 + 2];

            float r = y + cb - co;
            float g = y + co;
            float b = y - cb - co;

            pixels[i * 3] = r; // r
            pixels[i * 3 + 1] = g; // g
            pixels[i * 3 + 2] = b; // b
        }

        return pixels;
    }

    private float[] CmyFromRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float r = pixels[i * 3];
            float g = pixels[i * 3 + 1];
            float b = pixels[i * 3 + 2];

            float c = 255.0f - r;
            float m = 255.0f - g;
            float y = 255.0f - b;

            pixels[i * 3] = c; // c
            pixels[i * 3 + 1] = m; // m
            pixels[i * 3 + 2] = y; // y
        }

        return pixels;
    }

    private float[] CmyToRgb(float[] pixels)
    {
        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float c = pixels[i * 3];
            float m = pixels[i * 3 + 1];
            float y = pixels[i * 3 + 2];

            float r = 255.0f - c;
            float g = 255.0f - m;
            float b = 255.0f - y;

            pixels[i * 3] = r; // r
            pixels[i * 3 + 1] = g; // g
            pixels[i * 3 + 2] = b; // b
        }

        return pixels;
    }

    public ImageData ToRgb(ImageData source, ColorSpace colorSpace, bool[]? channels = default)
    {
        if (channels is {Length: not 3})
            throw new ArgumentException("Некорректный массив каналов");

        if (source.PixelFormat is PixelFormat.Gray)
        {
            if (colorSpace is not ColorSpace.Rgb)
                throw new ArgumentException("Серые изображения доступны только в цветовом пространстве RGB");

            source = source.SetPixelFormat(PixelFormat.Rgb);
        }

        var newPixels = (float[])source.Pixels.Clone();
        if (channels != null)
        {
            if (colorSpace is ColorSpace.YCbCr601 or ColorSpace.YCbCr709)
            {
                if (!channels[0])
                {
                    for (int j = 0; j < newPixels.Length; j += 3)
                    {
                        newPixels[j] = 16;
                    }    
                }
                
                if (!channels[1])
                {
                    for (int j = 1; j < newPixels.Length; j += 3)
                    {
                        newPixels[j] = 128;
                    }    
                }
                
                if (!channels[2])
                {
                    for (int j = 2; j < newPixels.Length; j += 3)
                    {
                        newPixels[j] = 128;
                    }    
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (channels[i]) continue;
                
                    for (int j = 0; j < newPixels.Length; j += 3)
                    {
                        newPixels[j + i] = 0;
                    }
                }
            }
        }

        newPixels = colorSpace switch
        {
            ColorSpace.Rgb => newPixels,
            ColorSpace.Hsl => HslToRgb(newPixels),
            ColorSpace.Hsv => HsvToRgb(newPixels),
            ColorSpace.YCbCr601 => YCbCr601ToRgb(newPixels),
            ColorSpace.YCbCr709 => YCbCr709ToRgb(newPixels),
            ColorSpace.YCoCg => YCbCoToRgb(newPixels),
            ColorSpace.Cmy => CmyToRgb(newPixels),
            _ => throw new ArgumentException("Цветовое пространство не поддерживается")
        };

        return new ImageData(newPixels, PixelFormat.Rgb, source.Height, source.Width);
    }

    public ImageData FromRgb(ImageData source, ColorSpace newColorSpace)
    {
        if (source.PixelFormat == PixelFormat.Gray)
        {
            source = source.SetPixelFormat(PixelFormat.Rgb);
        }

        float[] newPixels;
        newPixels = (float[])source.Pixels.Clone();
        
        newPixels = newColorSpace switch
        {
            ColorSpace.Rgb => newPixels,
            ColorSpace.Hsl => HslFromRgb(newPixels),
            ColorSpace.Hsv => HsvFromRgb(newPixels),
            ColorSpace.YCbCr601 => YCbCr601FromRgb(newPixels),
            ColorSpace.YCbCr709 => YCbCr709FromRgb(newPixels),
            ColorSpace.YCoCg => YCbCoFromRgb(newPixels),
            ColorSpace.Cmy => CmyFromRgb(newPixels),
            _ => throw new ArgumentException("Цветовое пространство не поддерживается")
        };

        return new ImageData(newPixels, PixelFormat.Rgb, source.Height, source.Width);
    }

    public ImageData Convert(ImageData source, ColorSpace currentColorSpace, ColorSpace newColorSpace)
    {
        return FromRgb(ToRgb(source, currentColorSpace), newColorSpace);
    }
}