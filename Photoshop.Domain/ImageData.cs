namespace Photoshop.Domain;

public record ImageData(byte[] Pixels, PixelFormat PixelFormat, int Height, int Width);