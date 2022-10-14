using System;
using System.Drawing.Printing;
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
        CanExecuteChanged += c_CanExecuteChanged;
        _dialogService = dialogService;
    }
    
    public async void Execute(object? parameter)
    {
        try
        {
            var path = await _dialogService.ShowSaveFileDialogAsync();
            if (path is null) return;

            PathCallback?.Invoke(path);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        };
    }

    private bool CanExecuteState = false;
    
    public bool CanExecute(object? parameter) => CanExecuteState;
    public event EventHandler? CanExecuteChanged;
    
    private void c_CanExecuteChanged(object? sender, EventArgs args)
    {
        CanExecuteState = true;
    } 

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}