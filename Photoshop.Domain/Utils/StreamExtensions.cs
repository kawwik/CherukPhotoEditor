using System.Text;
using Photoshop.Domain.Images;

namespace Photoshop.Domain.Utils;

public static class StreamExtensions
{
    public static void WriteChunk(this Stream stream, ChunkType chunkType, MemoryStream buffer)
    {
        var data = buffer.ToArray();
        buffer.Clear();
        
        if (chunkType == ChunkType.NOT_SUPPORTED)
        {
            return;
        }

        stream.WriteInt(data.Length);
        stream.Write(Encoding.ASCII.GetBytes(chunkType.ToString()));
        stream.Write(data);
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