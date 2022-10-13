using System.IO;
using Avalonia.Skia.Helpers;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.Domain.Images;
using Photoshop.Domain.Images.Factory;
using Photoshop.View.Commands;
using Photoshop.View.Converters;
using Photoshop.View.Extensions;
using ReactiveUI;
using IImage = Avalonia.Media.IImage;

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
        SaveImage.PathCallback = path =>
        {
            if (path.Length < 4)
                return; //Стоит ввести фидбек для пользователя
            string extension = path.Substring(path.Length - 4, 4).ToLower();
            Photoshop.Domain.Images.IImage image;
            switch (extension)
            {
                case ".pgm":
                    image = new PnmImage(imageEditor.GetData(), PixelFormat.Gray);
                    break;
                case ".ppm":
                    image = new PnmImage(imageEditor.GetData(), PixelFormat.Rgb);
                    break;
                default:
                    return;
            }
            var imageData = image.GetFile();
            using var fileStream = File.Open(path, FileMode.Create);
            fileStream.Write(imageData);
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