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
        GenerateGradient = commandFactory.GenerateGradient();

        var imageGamma = OpenImage.WhereNotNull()
            .Select(x => (double)x.Gamma);

        imageGamma
            .TakeWhenNot(GammaContext.ObservableForPropertyValue(x => x.IgnoreImageGamma))
            .Subscribe(x => GammaContext.InnerGamma = x)
            .AddTo(_subscriptions);

        var imageEditor = Observable.Merge(OpenImage, GenerateGradient)
            .WhereNotNull()
            .Select(imageData => imageEditorFactory.GetImageEditor(imageData, ColorSpaceContext.CurrentColorSpace));

        Image = Observable.CombineLatest(
            imageEditor,
            ColorSpaceContext.Channels,
            GammaContext.ObservableForPropertyValue(x => x.OutputGamma),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            (imageEditor, channels, outputGamma, ditheringType, ditheringDepth) =>
                imageEditor.GetRgbData((float)outputGamma, ditheringType, ditheringDepth, channels));

        InnerImage = Observable.CombineLatest(
            imageEditor,
            GammaContext.ObservableForPropertyValue(x => x.InnerGamma),
            ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            (imageEditor, gamma, colorSpace, ditheringType, ditheringDepth) =>
            {
                imageEditor.SetGamma((float)gamma);
                imageEditor.SetColorSpace(colorSpace);

                return imageEditor.GetDitheredData(ditheringType, ditheringDepth);
            });

        SaveImage = commandFactory.SaveImage(canExecute: InnerImage.Select(x => x is not null));

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

    private void OnError(Exception exception)
    {
        _logger.Log(this, $"Ошибка: {exception}", exception);

        _dialogService.ShowErrorAsync(exception.Message);
    }

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}