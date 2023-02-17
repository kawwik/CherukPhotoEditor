namespace Photoshop.Domain.Utils;

public class CrcCalculator
{
    private readonly ulong[] _crcTable = new ulong[256];
   
    /* Flag: has the table been computed? Initially false. */
    private int _crcTableComputed = 0;
   
    /* Make the table for a fast CRC. */
    private void MakeCrcTable()
    {
        ulong c;
        int n, k;
   
        for (n = 0; n < 256; n++) {
            c = (ulong) n;
            for (k = 0; k < 8; k++) {
                if ((c & 1) != 0)
                    c = 0xedb88320L ^ (c >> 1);
                else
                    c = c >> 1;
            }
            _crcTable[n] = c;
        }
        _crcTableComputed = 1;
    }

    private ulong UpdateCrc(ulong crc, ReadOnlySpan<byte> buf)
    {
        int len = buf.Length;
        ulong c = crc;
        int n;
   
        if (_crcTableComputed != 0)
            MakeCrcTable();
        for (n = 0; n < len; n++) {
            c = _crcTable[(c ^ buf[n]) & 0xff] ^ (c >> 8);
        }
        return c;
    }
   
    /* Return the CRC of the bytes buf[0..len-1]. */
    public int CalculateCrc(ReadOnlySpan<byte> buf)
    {
        return (int) (UpdateCrc(0xffffffffL, buf) ^ 0xffffffffL);
    }
}