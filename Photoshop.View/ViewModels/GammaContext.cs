using System;
using ReactiveUI;

namespace Photoshop.View.ViewModels;

public class GammaContext : ReactiveObject
{
    public const float DefaultGamma = 1;
    
    private double _innerGamma = 1;
    private double _outputGamma = 1;

    public double InnerGamma
    {
        get => _innerGamma;
        set => this.RaiseAndSetIfChanged(ref _innerGamma, Math.Round(value, 2));
    }

    public double OutputGamma
    {
        get => _outputGamma;
        set => this.RaiseAndSetIfChanged(ref _outputGamma, Math.Round(value, 2));
    }
}