using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Photoshop.Domain;
using Photoshop.Domain.ImageEditors;
using Photoshop.View.Services.Interfaces;
using Photoshop.View.Utils.Extensions;
using ReactiveUI;
using IAvaloniaImage = Avalonia.Media.IImage;

namespace Photoshop.View.ViewModels;

public class PhotoEditionContext : ReactiveObject, IDisposable
{
    private readonly IDialogService _dialogService;

    private readonly ObservableAsPropertyHelper<IImageEditor?> _imageEditor;

    private readonly List<IDisposable> _subscriptions = new();

    public PhotoEditionContext(
        ICommandFactory commandFactory,
        IDialogService dialogService,
        ColorSpaceContext colorSpaceContext,
        GammaContext gammaContext, DitheringContext ditheringContext)
    {
        _dialogService = dialogService;

        ColorSpaceContext = colorSpaceContext;
        GammaContext = gammaContext;
        DitheringContext = ditheringContext;

        OpenImage = commandFactory.OpenImage;
        SaveImage = commandFactory.SaveImage;

        _imageEditor = OpenImage.ToProperty(this, x => x.ImageEditor);
        _imageEditor.AddTo(_subscriptions);
        
        ImageData = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor),
            ColorSpaceContext.Channels,
            GammaContext.ObservableForPropertyValue(x => x.InnerGamma),
            GammaContext.ObservableForPropertyValue(x => x.OutputGamma),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            (imageEditor, channels, _, outputGamma, _, _) => imageEditor?.GetRgbData((float)outputGamma, imageEditor._ditheringType, imageEditor._ditheringDepth, channels));

        GammaContext.ObservableForPropertyValue(x => x.InnerGamma)
            .Subscribe(x => ImageEditor?.ConvertGamma((float)x))
            .AddTo(_subscriptions);
        
        ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace)
            .Subscribe(x => ImageEditor?.SetColorSpace(x))
            .AddTo(_subscriptions);

        Observable.Merge(OpenImage.ThrownExceptions, SaveImage.ThrownExceptions)
            .Subscribe(OnError)
            .AddTo(_subscriptions);
    }
    
    public IObservable<ImageData?> ImageData { get; }
    
    public ReactiveCommand<ColorSpace, IImageEditor?> OpenImage { get; }
    public ReactiveCommand<ImageData, Unit> SaveImage { get; }

    public ColorSpaceContext ColorSpaceContext { get; }
    public GammaContext GammaContext { get; }
    public DitheringContext DitheringContext { get; }

    private IImageEditor? ImageEditor => _imageEditor.Value;

    private void OnError(Exception exception)
    {
        var task = _dialogService.ShowErrorAsync(exception.Message);
        task.Wait();
    }

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}