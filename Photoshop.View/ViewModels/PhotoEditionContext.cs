using Avalonia.Media;
using Photoshop.Domain.ImageEditors;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.Domain.Images.Factory;
using Photoshop.View.Commands;
using Photoshop.View.Converters;
using ReactiveUI;

namespace Photoshop.View.ViewModels;
public class PhotoEditionContext : ReactiveObject
{
    private readonly IImageFactory _imageFactory;
    private readonly IImageEditorFactory _imageEditorFactory;
    private readonly IImageConverter _imageConverter;
    
    private IImageEditor _imageEditor;

    public PhotoEditionContext(
        OpenImageCommand openImage, 
        SaveImageCommand saveImage, 
        IImageFactory imageFactory, 
        IImageEditorFactory imageEditorFactory, 
        IImageConverter imageConverter, 
        IImageEditor imageEditor)
    {
        _imageFactory = imageFactory;
        _imageEditorFactory = imageEditorFactory;
        _imageConverter = imageConverter;
        _imageEditor = imageEditor;
        
        SaveImage = saveImage;
        OpenImage = openImage;
        OpenImage.StreamCallback = stream =>
        {
            var bytes = new byte[]{};
            stream.Read(bytes, 0, (int)stream.Length);
            var image = _imageFactory.GetImage(bytes);

            var imageData = image.GetData();
            ImageEditor = _imageEditorFactory.GetImageEditor(imageData);
        };
    }

    public OpenImageCommand OpenImage { get; }
    public SaveImageCommand SaveImage { get; }

    public IImageEditor ImageEditor
    {
        get => _imageEditor;
        set => _imageEditor = this.RaiseAndSetIfChanged(ref _imageEditor, value);
    }

    public IImage Image => _imageConverter.ConvertToBitmap(ImageEditor.GetData());
}