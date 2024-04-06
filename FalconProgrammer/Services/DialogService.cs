using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

/// <summary>
///   A service that can show different types of dialog windows.
/// </summary>
public class DialogService : IDialogService {
  private TopLevel? _topLevel;
  private TopLevel TopLevel => _topLevel ??= ((App)Application.Current!).MainWindow;

  public async Task<string?> BrowseForFileAsync(
    string dialogTitle, string filterName, string fileExtension) {
    string pattern = $"*.{fileExtension}";
    var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
      new FilePickerOpenOptions {
        Title = dialogTitle,
        AllowMultiple = false,
        // For macOS, we may need to specify
        // FilePickerOpenOptions.AppleUniformTypeIdentifiers in addition to 
        // FilePickerOpenOptions.FileTypeFilter, which is required for windows.
        FileTypeFilter = [
          new FilePickerFileType(filterName) {
            Patterns = [pattern]
          }
        ]
      });
    return files.Count == 1 ? files[0].Path.AbsolutePath : null;
  }

  public async Task<string?> BrowseForFolderAsync(string dialogTitle) {
    var folders =
      await TopLevel.StorageProvider.OpenFolderPickerAsync(
        new FolderPickerOpenOptions {
          Title = dialogTitle,
          AllowMultiple = false
        }
      );
    return folders.Count == 1 ? folders[0].Path.AbsolutePath : null;
  }

  public async Task ShowErrorMessageBoxAsync(string text) {
    await Task.Delay(0);
    // await DialogService.ShowMessageBoxAsync(ownerViewModel, text,
    //   Application.Current!.Name!,
    //   MessageBoxButton.Ok,
    //   MessageBoxImage.Error);
  }
}