﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Logging;
using CherukPhotoEditor.Domain;
using CherukPhotoEditor.Domain.ImageEditors;
using CherukPhotoEditor.Domain.ImageEditors.Factory;
using CherukPhotoEditor.View.Services.Interfaces;
using CherukPhotoEditor.View.ViewModels;
using CherukPhotoEditor.View.Utils.Extensions;
using ReactiveUI;

namespace CherukPhotoEditor.View;

public class PhotoEditionContext : ReactiveObject, IDisposable
{
    private readonly IDialogService _dialogService;

    private readonly List<IDisposable> _subscriptions = new();

    private readonly ParametrizedLogger _logger;
    private ObservableAsPropertyHelper<IImageEditor?> _imageEditor;

    public PhotoEditionContext(
        ICommandFactory commandFactory,
        IDialogService dialogService,
        ColorSpaceContext colorSpaceContext,
        GammaContext gammaContext,
        DitheringContext ditheringContext,
        IImageEditorFactory imageEditorFactory)
    {
        var logger = Logger.TryGet(LogEventLevel.Debug, nameof(PhotoEditionContext));

        if (logger is null)
            throw new Exception("Не удалось получить логгер");

        _logger = logger.Value;

        _dialogService = dialogService;

        ColorSpaceContext = colorSpaceContext;
        GammaContext = gammaContext;
        DitheringContext = ditheringContext;

        OpenImage = commandFactory.OpenImage();
        GenerateGradient = commandFactory.GenerateGradient();

        _imageEditor = Observable.Merge(OpenImage, GenerateGradient)
            .WhereNotNull()
            .WithLatestFrom(GammaContext.ObservableForPropertyValue(x => x.IgnoreImageGamma))
            .Select(args =>
            {
                var (imageData, ignoreImageGamma) = args;

                if (!ignoreImageGamma)
                {
                    GammaContext.InnerGamma = imageData.Gamma;
                    return imageData;
                }

                return new ImageData(
                    imageData.Pixels, 
                    imageData.PixelFormat, 
                    imageData.Height, 
                    imageData.Width, 
                    (float)GammaContext.InnerGamma);
            })
            .Select(data => imageEditorFactory.GetImageEditor(data, ColorSpaceContext.CurrentColorSpace))
            .ToProperty(this, x => x.ImageEditor);

        _imageEditor.AddTo(_subscriptions);
        
        Image = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor).WhereNotNull(),
            GammaContext.ObservableForPropertyValue(x => x.OutputGamma),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            ColorSpaceContext.Channels,
            (imageEditor, gamma, ditheringType, ditheringDepth, channels) =>
            {
                var result = imageEditor.GetRgbData((float)gamma, ditheringType, ditheringDepth, channels);

                return result;
            });
        
        InnerImage = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor).WhereNotNull(),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            GammaContext.ObservableForPropertyValue(x => x.InnerGamma),
            ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace),
            (imageEditor, ditheringType, ditheringDepth, gamma, colorSpace) =>
            {
                imageEditor.ConvertGamma((float)gamma);
                imageEditor.SetColorSpace(colorSpace);
                
                var result = imageEditor.GetDitheredData(ditheringType, ditheringDepth);

                return result;
            });

        SaveImage = commandFactory.SaveImage(canExecute: InnerImage.Any());

        Observable.Merge(
                OpenImage.ThrownExceptions,
                SaveImage.ThrownExceptions,
                GenerateGradient.ThrownExceptions)
            .Subscribe(OnError)
            .AddTo(_subscriptions);
    }

    public IImageEditor? ImageEditor => _imageEditor.Value;

    public IObservable<ImageData?> Image { get; }
    public IObservable<ImageData?> InnerImage { get; }

    public ReactiveCommand<ColorSpace, ImageData?> OpenImage { get; }
    public ReactiveCommand<ImageData, Unit> SaveImage { get; }
    public ReactiveCommand<Unit, ImageData> GenerateGradient { get; }

    public ColorSpaceContext ColorSpaceContext { get; }
    public GammaContext GammaContext { get; }
    public DitheringContext DitheringContext { get; }

    private void OnError(Exception exception)
    {
        _logger.Log(this, $"Ошибка: {exception}", exception);

        _dialogService.ShowErrorAsync(exception.Message);
    }

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}