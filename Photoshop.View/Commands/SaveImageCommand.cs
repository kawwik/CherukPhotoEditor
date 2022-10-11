using System;
using System.IO;
using System.Windows.Input;
using Photoshop.View.Services.Interfaces;

namespace Photoshop.View.Commands;

public class SaveImageCommand : ICommand
{
    private readonly IDialogService _dialogService;
    private bool _executableState = false;

    public void setExecutableState(bool state)
    {
        _executableState = state;
    }
    
    public Action<string>? PathCallback { get; set; }

    public SaveImageCommand(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public bool CanExecute(object? parameter) => _executableState;

    public void Execute(object? parameter)
    {
        var path = _dialogService.ShowSaveFileDialogAsync().Result;
        if (path is null) return;
        
        PathCallback?.Invoke(path);
    }

    public event EventHandler? CanExecuteChanged;
    
    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}