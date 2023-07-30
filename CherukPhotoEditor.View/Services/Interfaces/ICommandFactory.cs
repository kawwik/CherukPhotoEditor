using System;
using System.Reactive;
using CherukPhotoEditor.Domain;
using CherukPhotoEditor.Domain.ImageEditors;
using ReactiveUI;

namespace CherukPhotoEditor.View.Services.Interfaces;

public interface ICommandFactory
{
    ReactiveCommand<ColorSpace, ImageData?> OpenImage();
    ReactiveCommand<ImageData, Unit> SaveImage(IObservable<bool> canExecute);
    ReactiveCommand<Unit, ImageData> GenerateGradient();
}