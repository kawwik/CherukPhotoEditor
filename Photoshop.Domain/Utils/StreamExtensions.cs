using System.Text;
using Photoshop.Domain.Images;

namespace Photoshop.Domain.Utils;

public static class StreamExtensions
{
    public static async Task WriteChunkAsync(this Stream stream, ChunkType chunkType, MemoryStream buffer)
    {
        var data = buffer.ToArray();
        buffer.Clear();
        
        if (chunkType == ChunkType.NOT_SUPPORTED)
        {
            return;
        }

        stream.WriteInt(data.Length);
        await stream.WriteAsync(Encoding.ASCII.GetBytes(chunkType.ToString()));
        await stream.WriteAsync(data);
        stream.WriteInt(GetCRC(data));
    }

    private static void Clear(this Stream stream)
    {
        stream.SetLength(0);
    }
    
    private static int GetCRC(ReadOnlySpan<byte> data)
    {
        CrcCalculator calculator = new CrcCalculator();
        return calculator.CalculateCrc(data);
    }

    public static void WriteInt(this Stream stream, int value)
    {
        stream.WriteByte((byte) (value >> 24));
        stream.WriteByte((byte) (value >> 16 & 255));
        stream.WriteByte((byte) (value >> 8 & 255));
        stream.WriteByte((byte) (value & 255));
    }
}