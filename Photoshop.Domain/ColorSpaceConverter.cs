namespace Photoshop.Domain.Utils;

public class ColorSpaceConverter : IColorSpaceConverter
{
    public const float YCbCr601_k_r = 0.299f;
    public const float YCbCr601_k_g = 0.587f;
    public const float YCbCr601_k_b = 0.114f;

    public const float YCbCr709_k_r = 0.2126f;
    public const float YCbCr709_k_g = 0.7152f;
    public const float YCbCr709_k_b = 0.0722f;

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
                    pixels[i * 3] = coef * ((Math.Abs(g2 - b2) / delta) % 6.0f);
                else if (g2 == cMax)
                    pixels[i * 3] = coef * ((b2 - r2) / delta + 2.0f);
                else
                    pixels[i * 3] = coef * ((r2 - g2) / delta + 4.0f);

                // Lightness
                float l = (cMax + cMin) / 2.0f;
                pixels[i * 3 + 2] = l;

                // Saturation
                pixels[i * 3 + 1] = delta / (1.0f - Math.Abs(2.0f * l - 1));
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
            float s = pixels[i * 3 + 1];
            float l = pixels[i * 3 + 2];

            float c = (1 - Math.Abs(2 * l - 1)) * s;
            float x = c * (1 - Math.Abs((h / 60.0f) % 2 - 1));
            float m = l - c / 2.0f;

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
                    pixels[i * 3] = coef * (Math.Abs(g2 - b2) / delta % 6.0f);
                else if (g2 == cMax)
                    pixels[i * 3] = coef * ((b2 - r2) / delta + 2.0f);
                else
                    pixels[i * 3] = coef * ((r2 - g2) / delta + 4.0f);

                // Saturation
                pixels[i * 3 + 1] = cMax == 0 ? 0 : delta / cMax;

                // Value
                pixels[i * 3 + 2] = cMax;
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
            float s = pixels[i * 3 + 1];
            float v = pixels[i * 3 + 2];

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

    private float[] YCbCrFromRgb(float[] pixels, float k_r, float k_g, float k_b)
    {
        float k_cb = 2.0f * (k_r + k_g);
        float k_cr = 2.0f * (1.0f - k_r);

        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float r = pixels[i * 3];
            float g = pixels[i * 3 + 1];
            float b = pixels[i * 3 + 2];

            float y = k_r * r + k_g * g + k_b * b;
            float cb = (b - y) / k_cb;
            float cr = (r - y) / k_cr;

            pixels[i * 3] = y; // Y
            pixels[i * 3 + 1] = cb; // Cb
            pixels[i * 3 + 2] = cr; // Cr
        }

        return pixels;
    }

    private float[] YCbCrToRgb(float[] pixels, float k_r, float k_g, float k_b)
    {
        float k_cb = 2.0f * (k_r + k_g);
        float k_cr = 2.0f * (1.0f - k_r);

        for (int i = 0; i < pixels.Length / 3; i++)
        {
            float y = pixels[i * 3];
            float cb = pixels[i * 3 + 1];
            float cr = pixels[i * 3 + 2];

            float r = y + k_cr * cr;
            float g = y - (k_r * k_cr / k_g) * cr - (k_b * k_cb / k_g) * cb;
            float b = y + k_cb * cb;

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
            for (int i = 0; i < 3; i++)
            {
                if (channels[i]) continue;
                
                for (int j = 0; j < newPixels.Length; j += 3)
                {
                    newPixels[j + i] = 0;
                }
            }
        }

        newPixels = colorSpace switch
        {
            ColorSpace.Rgb => newPixels,
            ColorSpace.Hsl => HslToRgb(newPixels),
            ColorSpace.Hsv => HsvToRgb(newPixels),
            ColorSpace.YCbCr601 => YCbCrToRgb(newPixels, YCbCr601_k_r, YCbCr601_k_g, YCbCr601_k_b),
            ColorSpace.YCbCr709 => YCbCrToRgb(newPixels, YCbCr709_k_r, YCbCr709_k_g, YCbCr709_k_b),
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
            ColorSpace.YCbCr601 => YCbCrFromRgb(newPixels, YCbCr601_k_r, YCbCr601_k_g, YCbCr601_k_b),
            ColorSpace.YCbCr709 => YCbCrFromRgb(newPixels, YCbCr709_k_r, YCbCr709_k_g, YCbCr709_k_b),
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