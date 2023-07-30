using System.Threading.Tasks;

namespace CherukPhotoEditor.View.Services.Interfaces;

public interface IDialogService
{
    Task<string?> ShowOpenFileDialogAsync();
    Task<string?> ShowSaveFileDialogAsync();
    Task ShowErrorAsync(string message);
}