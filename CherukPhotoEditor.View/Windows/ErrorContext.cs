namespace CherukPhotoEditor.View.Windows;

public class ErrorContext
{
    public ErrorContext(string message)
    {
        Message = message;
    }

    public string Message { get; }
}