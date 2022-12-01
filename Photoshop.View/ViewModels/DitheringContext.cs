using System;
using Photoshop.Domain;
using ReactiveUI;

namespace Photoshop.View.ViewModels;

public class DitheringContext : ReactiveObject
{
    private DitheringType _ditheringType = DitheringType.None;
    private int _ditheringDepth = 0;

    public DitheringType[] DitheringTypes { get; } = Enum.GetValues<DitheringType>();

    public DitheringType DitheringType
    {
        get => _ditheringType;
        set => this.RaiseAndSetIfChanged(ref _ditheringType, value);
    }

    public int DitheringDepth
    {
        get => _ditheringDepth;
        set => this.RaiseAndSetIfChanged(ref _ditheringDepth, value);
    }
}