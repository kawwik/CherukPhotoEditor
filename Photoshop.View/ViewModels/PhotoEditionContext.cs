using Avalonia.Media;
using Photoshop.Domain.ImageEditors;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.Domain.Images.Factory;
using Photoshop.View.Commands;
using Photoshop.View.Converters;
using Photoshop.View.Extensions;
using ReactiveUI;

namespace Photoshop.View.ViewModels;
public class PhotoEditionContext : ReactiveObject
{
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
        _imageConverter = imageConverter;
        _imageEditor = imageEditor;
        
        SaveImage = saveImage;
        OpenImage = openImage;
        OpenImage.StreamCallback = stream =>
        {
            var bytes = stream.ReadToEnd();
            var image = imageFactory.GetImage(bytes);

            var imageData = image.GetData();
            ImageEditor = imageEditorFactory.GetImageEditor(imageData);
        };
    }

    public OpenImageCommand OpenImage { get; }
    public SaveImageCommand SaveImage { get; }

    public IImage Image => _imageConverter.ConvertToBitmap(ImageEditor.GetData());

    private IImageEditor ImageEditor
    {
        get => _imageEditor;
        set
        {
            _imageEditor = this.RaiseAndSetIfChanged(ref _imageEditor, value);
            SaveImage.OnCanExecuteChanged();
        }
    }
}