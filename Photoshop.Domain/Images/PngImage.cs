using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using Photoshop.Domain.Utils;

namespace Photoshop.Domain.Images;

public class PngImage : IImage
{
    private readonly ImageData _data;
    private float _gamma;

    private static readonly byte[] PngHeader = { 137, 80, 78, 71, 13, 10, 26, 10 };

    public PngImage(ImageData data, float gamma)
    {
        _data = data;
        _gamma = gamma;
    }

    public PngImage(byte[] image)
    {
        if (!CheckFileHeader(image))
        {
            throw new ArgumentException("Некорректный PNG файл");
        }

        var chunkTypeCnt = Enum.GetValues<ChunkType>()
            .ToDictionary(type => type, _ => 0);

        int ind = 8;
        int height = 0, width = 0, colorType = 0;
        PixelFormat pixelFormat = PixelFormat.Gray;
        byte[]? imageBytes = default;
        byte[]? palette = default;
        float gamma = 1;

        int bytesRead = 0;
        int coef = 1;

        var chunk = ReadChunk(image, ind);
        if (chunk.Type is not ChunkType.IHDR)
        {
            throw new ArgumentException("Некорректный PNG файл");
        }

        while (chunk.Type is not ChunkType.IEND)
        {
            // ChunkType.IEND не прибавит единицы, так как цикл while на нем завершится
            chunkTypeCnt[chunk.Type]++;

            switch (chunk.Type)
            {
                case ChunkType.IHDR:
                    width = ReadInt(image, chunk.DataStart);
                    height = ReadInt(image, chunk.DataStart + 4);
                    int bitDepth = image[chunk.DataStart + 8];
                    colorType = image[chunk.DataStart + 9];

                    if (bitDepth != 8)
                    {
                        throw new ArgumentException("Некорректный PNG файл");
                    }

                    if (colorType != 0 && colorType != 2 && colorType != 3)
                    {
                        throw new ArgumentException("Некорректный PNG файл");
                    }

                    pixelFormat = colorType == 0 ? PixelFormat.Gray : PixelFormat.Rgb;
                    coef = pixelFormat == PixelFormat.Gray ? 1 : 3;

                    imageBytes = new byte[(width * coef + 1) * height + 100];
                    break;

                case ChunkType.PLTE:
                    if (chunk.DataSize % 3 != 0 || chunk.DataSize > 256 * 3)
                    {
                        throw new ArgumentException("Некорректный PNG файл");
                    }

                    palette = new byte[chunk.DataSize];
                    Array.Copy(image, chunk.DataStart, palette, 0, chunk.DataSize);
                    break;

                case ChunkType.IDAT:
                    if (imageBytes is null)
                    {
                        throw new ArgumentException("Некорректный PNG файл");
                    }

                    using (var memoryStream = new MemoryStream(image, chunk.DataStart, chunk.DataSize))
                    using (var zlibStream = new ZLibStream(memoryStream, CompressionMode.Decompress))
                    {
                        int bytesWritten;
                        do
                        {
                            bytesWritten = zlibStream.Read(imageBytes, bytesRead, imageBytes.Length - bytesRead);
                            bytesRead += bytesWritten;
                        } while (bytesWritten > 0);


                        if (bytesRead > (width * coef + 1) * height)
                        {
                            throw new ArgumentException("Некорректный PNG файл");
                        }
                    }

                    break;

                case ChunkType.gAMA:
                    gamma = ReadInt(image, chunk.DataStart) / 100000f;
                    break;

                default:
                    throw new NotSupportedException("Тип чанка не поддерживается");
            }

            ind += chunk.TotalSize();

            chunk = ReadChunk(image, ind);
        }

        if (chunkTypeCnt[ChunkType.IHDR] != 1
            || chunkTypeCnt[ChunkType.gAMA] > 1
            || chunkTypeCnt[ChunkType.IDAT] == 0
            || chunkTypeCnt[ChunkType.PLTE] > (colorType == 0 ? 0 : 1))
        {
            throw new ArgumentException("Некорректный PNG файл");
        }

        coef = colorType == 2 ? 3 : 1;

        if (bytesRead != height * (width * coef + 1))
        {
            throw new ArgumentException("Некорректный PNG файл" + bytesRead + " " + height + " " + width + " " + coef +
                                        " " + colorType);
        }

        float[] pixels = new float[height * width * coef];

        if (imageBytes is null)
        {
            throw new ArgumentException("Некорректный PNG файл");
        }

        if (colorType == 3)
        {
            if (palette is null)
            {
                throw new ArgumentException("Некорректный PNG файл");
            }

            pixels = new float[height * width * 3];

            for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                // (width + 1) и +1 возникают из-за байта фильтрации в начале каждой строки
                pixels[(i * width + j) * 3] = palette[imageBytes[i * (width + 1) + j + 1] * 3];
                pixels[(i * width + j) * 3 + 1] = palette[imageBytes[i * (width + 1) + j + 1] * 3 + 1];
                pixels[(i * width + j) * 3 + 2] = palette[imageBytes[i * (width + 1) + j + 1] * 3 + 2];
            }
        }
        else
        {
            for (int i = 0; i < height; i++)
            for (int j = 0; j < width * coef; j++)
            {
                pixels[i * width * coef + j] = imageBytes[i * (width * coef + 1) + j + 1];
            }
        }

        var noGammaImageData = new ImageData(pixels, pixelFormat, height, width);
        _data = new GammaConverter().ConvertGamma(noGammaImageData, gamma, 1);
    }

    public static bool CheckFileHeader(byte[] image)
    {
        for (int i = 0; i < PngHeader.Length; i++)
        {
            if (image[i] != PngHeader[i])
                return false;
        }

        return true;
    }

    public ImageData GetData()
    {
        return  _data;
    }

    public byte[] GetFile()
    {
        using var outputStream = new MemoryStream();
        using var buffer = new MemoryStream();

        outputStream.Write(PngHeader);

        buffer.WriteInt(_data.Width);
        buffer.WriteInt(_data.Height);

        var pixelFormat = _data.PixelFormat == PixelFormat.Gray ? (byte)0 : (byte)2;
        buffer.Write(new byte[]
        {
            8, pixelFormat, 0, 0, 0
        });
        
        outputStream.WriteChunk(ChunkType.IHDR, buffer);

        var bufferArray = new byte[_data.Pixels.Length + _data.Height]; 
        int bytesPerPixel = _data.PixelFormat == PixelFormat.Gray ? 1 : 3;

        for (int i = 0; i < _data.Height; i++)
        for (int j = 0; j < _data.Width * bytesPerPixel; j++)
        {
            bufferArray[i * (_data.Width * bytesPerPixel + 1) + j + 1] = (byte)(_data.Pixels[_data.Width * bytesPerPixel * i + j]);
        }

        using (var compressedDataStream = new MemoryStream())
        using (var zlibStream = new ZLibStream(compressedDataStream, CompressionMode.Compress))
        {
            zlibStream.Write(bufferArray);
            zlibStream.Flush();
            
            compressedDataStream.Position = 0;
            outputStream.WriteChunk(ChunkType.IDAT, compressedDataStream);
        }

        buffer.WriteInt((int)_gamma * 100000);
        
        outputStream.WriteChunk(ChunkType.gAMA, buffer);
        outputStream.WriteChunk(ChunkType.IEND, buffer);

        return outputStream.ToArray();
    }

    private record ChunkInfo (int DataSize, int DataStart, ChunkType Type)
    {
        public int TotalSize()
        {
            return DataSize + 12;
        }
    }

    private int ReadInt(byte[] image, int ind)
    {
        return (1 << 24) * image[ind] + (1 << 16) * image[ind + 1] + (1 << 8) * image[ind + 2] + image[ind + 3];
    }

    private ChunkInfo ReadChunk(byte[] image, int chunkStart)
    {
        int size = ReadInt(image, chunkStart);
        var chunkTypeStr = Encoding.ASCII.GetString(image, chunkStart + 4, 4);

        bool result = Enum.TryParse(typeof(ChunkType), chunkTypeStr, out var chunkType);
        if (result)
            return new ChunkInfo(size, chunkStart + 8, (ChunkType)chunkType!);
        else
            return new ChunkInfo(size, chunkStart + 8, ChunkType.NOT_SUPPORTED);
    }
}