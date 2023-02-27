using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Logging;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.Domain.ImageEditors.Factory;
using Photoshop.View.Services.Interfaces;
using Photoshop.View.Utils.Extensions;
using Photoshop.View.ViewModels;
using ReactiveUI;

namespace Photoshop.View;

public class PhotoEditionContext : ReactiveObject, IDisposable
{
    private readonly IDialogService _dialogService;

    private readonly ObservableAsPropertyHelper<IImageEditor?> _imageEditor;

    private readonly List<IDisposable> _subscriptions = new();

    private readonly ParametrizedLogger _logger;

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
        SaveImage = commandFactory.SaveImage(canExecute: OpenImage.Select(x => x is not null));
        GenerateGradient = commandFactory.GenerateGradient();

        InnerGamma = Observable.Merge(
            GammaContext.ObservableForPropertyValue(x => x.InnerGamma),
            OpenImage.Where(x => x is { Gamma: not null })
                .Select(x => x!.Gamma!.Value));

        InnerGamma.Subscribe(x => GammaContext.InnerGamma = x).AddTo(_subscriptions);

        _imageEditor = Observable.Merge(OpenImage, GenerateGradient)
            .CombineLatest(InnerGamma)
            .DistinctUntilChanged(args => args.First)
            .Select(args =>
            {
                var (imageData, gamma) = args;

                return imageData is null
                    ? null
                    : imageEditorFactory.GetImageEditor(imageData, ColorSpaceContext.CurrentColorSpace, (float)gamma);
            })
            .Where(x => x is not null)
            .ToProperty(this, x => x.ImageEditor);

        _imageEditor.AddTo(_subscriptions);

        Image = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor),
            ColorSpaceContext.Channels,
            GammaContext.ObservableForPropertyValue(x => x.OutputGamma),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            (imageEditor, channels, outputGamma, ditheringType, ditheringDepth) =>
                imageEditor?.GetRgbData((float)outputGamma, ditheringType, ditheringDepth, channels));

        InnerImage = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor),
            InnerGamma,
            ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            (imageEditor, _, _, ditheringType, ditheringDepth) =>
                imageEditor?.GetDitheredData(ditheringType, ditheringDepth)
        );

        GammaContext.ObservableForPropertyValue(x => x.InnerGamma)
            .Subscribe(x => ImageEditor?.SetGamma((float)x))
            .AddTo(_subscriptions);

        ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace)
            .Subscribe(x => ImageEditor?.SetColorSpace(x))
            .AddTo(_subscriptions);

        Observable.Merge(
                OpenImage.ThrownExceptions,
                SaveImage.ThrownExceptions,
                GenerateGradient.ThrownExceptions)
            .Subscribe(OnError)
            .AddTo(_subscriptions);
    }

    public IObservable<ImageData?> Image { get; }
    public IObservable<ImageData?> InnerImage { get; }

    public ReactiveCommand<ColorSpace, ImageData?> OpenImage { get; }
    public ReactiveCommand<ImageData, Unit> SaveImage { get; }
    public ReactiveCommand<Unit, ImageData> GenerateGradient { get; }

    public ColorSpaceContext ColorSpaceContext { get; }
    public GammaContext GammaContext { get; }
    public DitheringContext DitheringContext { get; }

    public IObservable<double> InnerGamma { get; }
    private IImageEditor? ImageEditor => _imageEditor.Value;

    private void OnError(Exception exception)
    {
        _logger.Log(this, $"Ошибка: {exception}", exception);

        _dialogService.ShowErrorAsync(exception.Message);
    }

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}