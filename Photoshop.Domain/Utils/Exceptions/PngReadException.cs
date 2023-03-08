namespace Photoshop.Domain.Utils.Exceptions;

public class PngReadException : Exception
{
    public PngReadException(string message) : base($"Ошибка чтения PNG: {message}")
    {
        
    }
}