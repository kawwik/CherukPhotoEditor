using Photoshop.View.Commands;

namespace Photoshop.View.ViewModels;

public class PhotoEditionContext
{
    public PhotoEditionContext(OpenImageCommand openImage, SaveImageCommand saveImage)
    {
        OpenImage = openImage;
        SaveImage = saveImage;
    }

    public OpenImageCommand OpenImage { get; }
    public SaveImageCommand SaveImage { get; }
}