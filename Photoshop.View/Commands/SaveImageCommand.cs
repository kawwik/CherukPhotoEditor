using System;
using System.IO;
using System.Windows.Input;
using Photoshop.View.Services.Interfaces;

namespace Photoshop.View.Commands;

public class SaveImageCommand : ICommand
{
    private readonly IDialogService _dialogService;
    
    public Action<string>? PathCallback { get; set; }

    public SaveImageCommand(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public bool CanExecute(object? parameter) => true;

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