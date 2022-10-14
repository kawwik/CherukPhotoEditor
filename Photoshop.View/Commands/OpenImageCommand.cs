using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Photoshop.View.Services.Interfaces;

namespace Photoshop.View.Commands;

public class OpenImageCommand : ICommand
{
    private readonly IDialogService _dialogService;
    
    public Func<Stream, Task>? StreamCallback { get; set; }

    public OpenImageCommand(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public bool CanExecute(object? parameter) => true;

    public async void Execute(object? parameter)
    {
        try
        {
            var path = await _dialogService.ShowOpenFileDialogAsync();
            if (path is null) return;
            using var fileStream = File.Open(path, FileMode.Open);
            StreamCallback?.Invoke(fileStream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        };
    }

    public event EventHandler? CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}