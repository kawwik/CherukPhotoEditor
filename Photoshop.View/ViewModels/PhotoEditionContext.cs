using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        CommandFactory commandFactory,
        IDialogService dialogService,
        ColorSpaceContext colorSpaceContext,
        GammaContext gammaContext)
    {
        _dialogService = dialogService;

        ColorSpaceContext = colorSpaceContext;
        GammaContext = gammaContext;

        OpenImage = commandFactory.OpenImage;
        SaveImage = commandFactory.SaveImage;

        _imageEditor = OpenImage.ToProperty(this, x => x.ImageEditor);
        _imageEditor.AddTo(_subscriptions);
        
        ImageData = Observable.CombineLatest(
            this.ObservableForPropertyValue(x => x.ImageEditor),
            ColorSpaceContext.Channels,
            GammaContext.ObservableForPropertyValue(x => x.InnerGamma),
            GammaContext.ObservableForPropertyValue(x => x.OutputGamma),
            (imageEditor, channels, _, outputGamma) => imageEditor?.GetRgbData((float)outputGamma, channels));

        GammaContext.ObservableForPropertyValue(x => x.InnerGamma)
            .Subscribe(x => ImageEditor?.ConvertGamma((float)x))
            .AddTo(_subscriptions);
        
        ColorSpaceContext.ObservableForPropertyValue(x => x.CurrentColorSpace)
            .Subscribe(x => ImageEditor?.SetColorSpace(x))
            .AddTo(_subscriptions);

        Observable.Merge(OpenImage.ThrownExceptions, SaveImage.ThrownExceptions)
            .Subscribe(x => OnError(x))
            .AddTo(_subscriptions);
    }
    
    public IObservable<ImageData?> ImageData { get; }
    
    public ReactiveCommand<ColorSpace, IImageEditor?> OpenImage { get; }
    public ReactiveCommand<ImageData, Unit> SaveImage { get; }

    public ColorSpaceContext ColorSpaceContext { get; }
    public GammaContext GammaContext { get; }

    private IImageEditor? ImageEditor => _imageEditor.Value;

    private Task OnError(Exception exception)
    {
        return _dialogService.ShowErrorAsync(exception.Message);
    }

    public void Dispose() => _subscriptions.ForEach(x => x.Dispose());
}