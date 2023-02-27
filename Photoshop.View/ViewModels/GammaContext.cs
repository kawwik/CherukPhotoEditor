using System;
using ReactiveUI;

namespace Photoshop.View.ViewModels;

public class GammaContext : ReactiveObject
{
    private double _innerGamma = 1;
    private double _outputGamma = 1;
    private bool _ignoreImageGamma;

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

    public bool IgnoreImageGamma
    {
        get => _ignoreImageGamma;
        set => this.RaiseAndSetIfChanged(ref _ignoreImageGamma, value);
    }
}