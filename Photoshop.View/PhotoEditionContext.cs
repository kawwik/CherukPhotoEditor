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

        var imageData = Observable.Merge(OpenImage, GenerateGradient)
            .WhereNotNull()
            .WithLatestFrom(GammaContext.ObservableForPropertyValue(x => x.IgnoreImageGamma))
            .Select(args =>
            {
                var (data, ignoreImageGamma) = args;

                if (!ignoreImageGamma)
                {
                    GammaContext.InnerGamma = data.Gamma;
                    return data;
                }

                return new ImageData(data.Pixels, data.PixelFormat, data.Height, data.Width, (float)GammaContext.InnerGamma);
            });

        var imageEditor = imageData
            .Select(data => imageEditorFactory.GetImageEditor(data, ColorSpaceContext.CurrentColorSpace));

        _imageEditor = imageEditor.ToProperty(this, x => x.ImageEditor);
        _imageEditor.AddTo(_subscriptions);
        
        Image = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor).WhereNotNull(),
            GammaContext.ObservableForPropertyValue(x => x.OutputGamma),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            ColorSpaceContext.Channels,
            (editor, gamma, ditheringType, ditheringDepth, channels) =>
            {
                var result = editor.GetRgbData((float)gamma, ditheringType, ditheringDepth, channels);

                return result;
            });
        
        InnerImage = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor).WhereNotNull(),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            GammaContext.ObservableForPropertyValue(x => x.InnerGamma),
            ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace),
            (editor, ditheringType, ditheringDepth, gamma, colorSpace) =>
            {
                editor.ConvertGamma((float)gamma);
                editor.SetColorSpace(colorSpace);
                
                var result = editor.GetDitheredData(ditheringType, ditheringDepth);

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