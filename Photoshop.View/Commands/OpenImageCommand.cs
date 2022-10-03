using System;
using System.Windows.Input;

namespace Photoshop.View.Commands;

public class OpenImageCommand : ICommand
{
    public bool CanExecute(object? parameter)
    {
        throw new NotImplementedException();
    }

    public void Execute(object? parameter)
    {
        throw new NotImplementedException();
    }

    public event EventHandler? CanExecuteChanged;
}