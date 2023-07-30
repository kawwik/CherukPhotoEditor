using System.IO;

namespace CherukPhotoEditor.View.Utils.Extensions;

public static class StreamExtensions
{
    public static byte[] ReadToEnd(this Stream stream)
    {
        var bytes = new byte[stream.Length];
        int offset = 0;
        while (stream.Position != stream.Length - 1)
        {
            offset += stream.Read(bytes, offset, (int)stream.Length);
        }

        return bytes;
    }
}