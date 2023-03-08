namespace Photoshop.Domain.Images;

public enum ChunkType
{
    IHDR,
    PLTE,
    IDAT,
    IEND,
    gAMA,
    NOT_SUPPORTED
}