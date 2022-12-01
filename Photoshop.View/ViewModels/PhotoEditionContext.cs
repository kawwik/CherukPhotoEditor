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
        GammaContext gammaContext, 
        DitheringContext ditheringContext)
    {
        _dialogService = dialogService;

        ColorSpaceContext = colorSpaceContext;
        GammaContext = gammaContext;
        DitheringContext = ditheringContext;

        OpenImage = commandFactory.OpenImage();
        SaveImage = commandFactory.SaveImage(OpenImage.Select(x => x is not null));
        GenerateGradient = commandFactory.GenerateGradient();

        _imageEditor = Observable.Merge(OpenImage, GenerateGradient).ToProperty(this, x => x.ImageEditor);
        _imageEditor.AddTo(_subscriptions);
        
        Image = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor),
            ColorSpaceContext.Channels,
            GammaContext.ObservableForPropertyValue(x => x.OutputGamma),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            (imageEditor, channels, outputGamma, ditheringType, ditheringDepth) => imageEditor?.GetRgbData((float)outputGamma, ditheringType, ditheringDepth, channels));

        InnerImage = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor),
            GammaContext.ObservableForPropertyValue(x => x.InnerGamma),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringType),
            DitheringContext.ObservableForPropertyValue(x => x.DitheringDepth),
            (imageEditor, _, ditheringType, ditheringDepth) => imageEditor?.GetDitheredData(ditheringType, ditheringDepth)
        );

        GammaContext.ObservableForPropertyValue(x => x.InnerGamma)
            .Subscribe(x =>
            {
                ImageEditor?.SetGamma((float)x);
                this.RaisePropertyChanged(nameof(ImageEditor));
            })
            .AddTo(_subscriptions);
        
        ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace)
            .Subscribe(x =>
            {
                ImageEditor?.SetColorSpace(x);
                this.RaisePropertyChanged(nameof(ImageEditor));
            })
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

    public ReactiveCommand<ColorSpace, IImageEditor?> OpenImage { get; }
    public ReactiveCommand<ImageData, Unit> SaveImage { get; }
    public ReactiveCommand<Unit, IImageEditor> GenerateGradient { get; }

    public ColorSpaceContext ColorSpaceContext { get; }
    public GammaContext GammaContext { get; }
    public DitheringContext DitheringContext { get; }

    private IImageEditor? ImageEditor => _imageEditor.Value;

    private void OnError(Exception exception)
    {
        _dialogService.ShowErrorAsync(exception.Message);
    }

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}