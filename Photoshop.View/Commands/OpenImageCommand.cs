using System;
using System.IO;
using System.Windows.Input;
using Photoshop.View.Services.Interfaces;

namespace Photoshop.View.Commands;

public class OpenImageCommand : ICommand
{
    private readonly IDialogService _dialogService;

    public OpenImageCommand(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        var path = _dialogService.ShowOpenFileDialogAsync().Result;
        if (path is null) return;

        using var fileStream = File.Open(path, FileMode.Open);
        
        fileStream.Dispose();
    }

    public event EventHandler? CanExecuteChanged;
}