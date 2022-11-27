using ReactiveUI;

namespace Photoshop.View.ViewModels;

public class Channel : ReactiveObject
{
    private string _name;
    private bool _value;

    public Channel(string name, bool value)
    {
        _name = name;
        _value = value;
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public bool Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
}