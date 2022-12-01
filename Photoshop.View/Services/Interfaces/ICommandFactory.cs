using System;
using System.Reactive;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using ReactiveUI;

namespace Photoshop.View.Services.Interfaces;

public interface ICommandFactory
{
    ReactiveCommand<ColorSpace, IImageEditor?> OpenImage();
    ReactiveCommand<ImageData, Unit> SaveImage(IObservable<bool> canExecute);
    ReactiveCommand<Unit, IImageEditor> GenerateGradient();
}