using System;
using CherukPhotoEditor.Domain;
using ReactiveUI;

namespace CherukPhotoEditor.View.ViewModels;

public class DitheringContext : ReactiveObject
{
    private DitheringType _ditheringType = DitheringType.None;
    private int _ditheringDepth = 8;

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