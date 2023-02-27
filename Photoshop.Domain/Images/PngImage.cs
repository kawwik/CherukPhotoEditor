using System.IO.Compression;
using System.Text;
using Photoshop.Domain.Utils;
using Photoshop.Domain.Utils.Exceptions;

namespace Photoshop.Domain.Images;

public record PngMetadata(int Width, int Height, int ColorType, PixelFormat PixelFormat, int BytesPerPixel);

public class PngImage : IImage
{
    private const int GammaCoefficient = 100000;
    private readonly ImageData _data;

    private static readonly byte[] PngHeader = { 137, 80, 78, 71, 13, 10, 26, 10 };

    public PngImage(ImageData data)
    {
        _data = data;
    }

    public PngImage(byte[] image)
    {
        if (!CheckFileHeader(image))
        {
            throw new PngReadException("Файл не является PNG");
        }

        var chunkTypeCnt = Enum.GetValues<ChunkType>()
            .ToDictionary(type => type, _ => 0);

        int ind = 8;
        int bytesRead = 0;

        var chunk = ReadChunk(image, ind);
        if (chunk.Type is not ChunkType.IHDR)
        {
            throw new PngReadException($"Первый чанк не {nameof(ChunkType.IHDR)}");
        }

        byte[]? imageBytes = default;
        byte[]? palette = default;
        PngMetadata? metadata = default;
        int? gamma = default;

        while (chunk.Type is not ChunkType.IEND)
        {
            // ChunkType.IEND не прибавит единицы, так как цикл while на нем завершится
            chunkTypeCnt[chunk.Type]++;

            switch (chunk.Type)
            {
                case ChunkType.IHDR:
                    (metadata, imageBytes) = ReadIHDR(image, chunk);
                    break;

                case ChunkType.PLTE:
                    palette = ReadPLTE(image, chunk);
                    break;

                case ChunkType.IDAT:
                    ReadIDAT(image, imageBytes!, chunk, ref bytesRead, metadata!);
                    break;

                case ChunkType.gAMA:
                    gamma = ReadInt(image, chunk.DataStart);
                    break;
            }

            ind += chunk.TotalSize();

            chunk = ReadChunk(image, ind);
        }

        var (width, height, colorType, pixelFormat, bytesPerPixel) = metadata!;

        if (chunkTypeCnt[ChunkType.IHDR] != 1
            || chunkTypeCnt[ChunkType.gAMA] > 1
            || chunkTypeCnt[ChunkType.IDAT] == 0
            || chunkTypeCnt[ChunkType.PLTE] > (colorType == 0 ? 0 : 1))
        {
            throw new PngReadException("Неверное количество чанков");
        }

        if (bytesRead != height * (width * bytesPerPixel + 1))
        {
            throw new PngReadException("Количество считанных байтов не совпадает с ожидаемым значением. " +
                                       $"Считано: {bytesRead}, Height = {height}, Width = {width}," +
                                       $" BytesPerPixel = {bytesPerPixel}, ColorType = {colorType}");
        }

        float[] pixels = new float[height * width * bytesPerPixel];

        if (colorType == 3)
        {
            if (palette is null)
            {
                throw new PngReadException("Не считан Palette");
            }

            pixels = new float[height * width * 3];

            for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                // (width + 1) и +1 возникают из-за байта фильтрации в начале каждой строки
                pixels[(i * width + j) * 3] = palette[imageBytes![i * (width + 1) + j + 1] * 3];
                pixels[(i * width + j) * 3 + 1] = palette[imageBytes[i * (width + 1) + j + 1] * 3 + 1];
                pixels[(i * width + j) * 3 + 2] = palette[imageBytes[i * (width + 1) + j + 1] * 3 + 2];
            }
        }
        else
        {
            for (int i = 0; i < height; i++)
            for (int j = 0; j < width * bytesPerPixel; j++)
            {
                pixels[i * width * bytesPerPixel + j] = imageBytes![i * (width * bytesPerPixel + 1) + j + 1];
            }
        }

        _data = new ImageData(pixels, pixelFormat, height, width, (double?)gamma / GammaCoefficient);
    }

    private static void ReadIDAT(byte[] image, byte[] imageBytes, ChunkInfo chunk, ref int bytesRead,
        PngMetadata metadata)
    {
        using var memoryStream = new MemoryStream(image, chunk.DataStart, chunk.DataSize);
        using var zlibStream = new ZLibStream(memoryStream, CompressionMode.Decompress);

        int bytesWritten;
        do
        {
            bytesWritten = zlibStream.Read(imageBytes, bytesRead, imageBytes.Length - bytesRead);
            bytesRead += bytesWritten;
        } while (bytesWritten > 0);


        var (width, height, _, _, bytesPerPixel) = metadata;

        if (bytesRead > (width * bytesPerPixel + 1) * height)
        {
            throw new PngReadException("Считано больше байтов, чем ожидалось");
        }
    }

    private static byte[] ReadPLTE(byte[] image, ChunkInfo chunk)
    {
        if (chunk.DataSize % 3 != 0 || chunk.DataSize > 256 * 3)
        {
            throw new PngReadException($"Неверный размер чанка {nameof(ChunkType.PLTE)}");
        }

        var palette = new byte[chunk.DataSize];
        Array.Copy(image, chunk.DataStart, palette, 0, chunk.DataSize);

        return image[chunk.DataStart..(chunk.DataStart + chunk.DataSize)];
    }

    private (PngMetadata, byte[]) ReadIHDR(byte[] image, ChunkInfo chunk)
    {
        var width = ReadInt(image, chunk.DataStart);
        var height = ReadInt(image, chunk.DataStart + 4);

        int bitDepth = image[chunk.DataStart + 8];

        if (bitDepth != 8)
        {
            throw new PngReadException($"BitDepth не равна 8. Фактическое значение: {bitDepth}");
        }

        int colorType = image[chunk.DataStart + 9];

        var (pixelFormat, bytesPerPixel) = colorType switch
        {
            0 => (PixelFormat.Gray, 1),
            2 or 3 => (PixelFormat.Rgb, 3),
            _ => throw new PngReadException($"Неподдерживаемый ColorType: {colorType}")
        };

        var imageBytes = new byte[(width * bytesPerPixel + 1) * height + 100];
        var metadata = new PngMetadata(width, height, colorType, pixelFormat, bytesPerPixel);

        return (metadata, imageBytes);
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
        return _data;
    }

    public async Task<byte[]> GetFileAsync()
    {
        await using var outputStream = new MemoryStream();
        await using var buffer = new MemoryStream();

        await outputStream.WriteAsync(PngHeader);

        buffer.WriteInt(_data.Width);
        buffer.WriteInt(_data.Height);

        var pixelFormat = _data.PixelFormat == PixelFormat.Gray ? (byte)0 : (byte)2;
        buffer.Write(new byte[]
        {
            8, pixelFormat, 0, 0, 0
        });

        await outputStream.WriteChunkAsync(ChunkType.IHDR, buffer);

        var bufferArray = new byte[_data.Pixels.Length + _data.Height];
        int bytesPerPixel = _data.PixelFormat == PixelFormat.Gray ? 1 : 3;

        for (int i = 0; i < _data.Height; i++)
        for (int j = 0; j < _data.Width * bytesPerPixel; j++)
        {
            bufferArray[i * (_data.Width * bytesPerPixel + 1) + j + 1] =
                (byte)(_data.Pixels[_data.Width * bytesPerPixel * i + j]);
        }

        await using (var compressedDataStream = new MemoryStream())
        await using (var zlibStream = new ZLibStream(compressedDataStream, CompressionMode.Compress))
        {
            await zlibStream.WriteAsync(bufferArray);
            await zlibStream.FlushAsync();

            compressedDataStream.Position = 0;
            await outputStream.WriteChunkAsync(ChunkType.IDAT, compressedDataStream);
        }

        if (_data.Gamma is null)
            throw new ArgumentException("Не установлена гамма изображения");
        
        buffer.WriteInt((int)(_data.Gamma * GammaCoefficient / 2.2));

        await outputStream.WriteChunkAsync(ChunkType.gAMA, buffer);
        await outputStream.WriteChunkAsync(ChunkType.IEND, buffer);

        return outputStream.ToArray();
    }

    private record ChunkInfo(int DataSize, int DataStart, ChunkType Type)
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

        return result
            ? new ChunkInfo(size, chunkStart + 8, (ChunkType)chunkType!)
            : new ChunkInfo(size, chunkStart + 8, ChunkType.NOT_SUPPORTED);
    }
}