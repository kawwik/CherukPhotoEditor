using System;
using System.IO;
using System.Windows.Input;
using Photoshop.View.Services.Interfaces;

namespace Photoshop.View.Commands;

public class SaveImageCommand : ICommand
{
    private readonly IDialogService _dialogService;
    private bool _executableState = false;

    public void SetExecutableState(bool state)
    {
        _executableState = state;
    }
    
    public Action<string>? PathCallback { get; set; }

    public SaveImageCommand(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public bool CanExecute(object? parameter) => true; //_executableState;

    public async void Execute(object? parameter)
    {
        try {
            var path = await _dialogService.ShowSaveFileDialogAsync();
            if (path is null) return;
            
            PathCallback?.Invoke(path);
            
        } catch (Exception e) {};
    }

    public event EventHandler? CanExecuteChanged;
    
    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}