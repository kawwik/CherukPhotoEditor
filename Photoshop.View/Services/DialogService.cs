using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Photoshop.View.Services.Interfaces;

namespace Photoshop.View.Services;

public class DialogService : IDialogService
{
    private readonly Window _parentWindow;

    private readonly List<FileDialogFilter> _fileDialogFilters = new()
    {
        new FileDialogFilter
        {
            Name = "JPEG image",
            Extensions = new List<string> { "jpeg" }
        }
    };

    public DialogService(Window parentWindow)
    {
        _parentWindow = parentWindow;
    }

    public async Task<string?> ShowOpenFileDialogAsync()
    {
        var dialog = new OpenFileDialog
        {
            AllowMultiple = false,
            // Filters = _fileDialogFilters
        };

        var result = await dialog.ShowAsync(_parentWindow).ConfigureAwait(false);
        return result?[0];
    }

    public async Task<string?> ShowSaveFileDialogAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filters = _fileDialogFilters
        };

        return await dialog.ShowAsync(_parentWindow).ConfigureAwait(false);
    }
}