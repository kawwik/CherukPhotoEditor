﻿namespace CherukPhotoEditor.Domain.ImageEditors.Factory;

public interface IImageEditorFactory
{
    IImageEditor GetImageEditor(ImageData imageData, ColorSpace colorSpace);
}