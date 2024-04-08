using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FalconProgrammer.ViewModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace FalconProgrammer.Services;

/// <summary>
///   A service that can show different types of dialog windows.
/// </summary>
public class DialogService : IDialogService {
  private string? _applicationTitle;
  private TopLevel? _topLevel;
  private string ApplicationTitle => _applicationTitle ??= Application.Current!.Name!;
  private TopLevel TopLevel => _topLevel ??= ((App)Application.Current!).MainWindow;

  public async Task<string?> BrowseForFileAsync(
    string dialogTitle, string filterName, string fileExtension) {
    string pattern = $"*.{fileExtension}";
    var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
      new FilePickerOpenOptions {
        Title = dialogTitle,
        AllowMultiple = false,
        // For macOS, we may need to specify
        // FilePickerOpenOptions.AppleUniformTypeIdentifiers either as well as or instead
        // of FilePickerOpenOptions.FileTypeFilter, which is required for windows.
        FileTypeFilter = [
          new FilePickerFileType(filterName) {
            Patterns = [pattern]
          }
        ]
      });
    return files.Count == 1 ? files[0].Path.LocalPath : null;
  }

  public async Task<string?> BrowseForFolderAsync(string dialogTitle) {
    var folders =
      await TopLevel.StorageProvider.OpenFolderPickerAsync(
        new FolderPickerOpenOptions {
          Title = dialogTitle,
          AllowMultiple = false
        }
      );
    return folders.Count == 1 ? folders[0].Path.LocalPath : null;
  }

  public async Task ShowErrorMessageBoxAsync(string text) {
    var messageBox = MessageBoxManager.GetMessageBoxStandard(
      ApplicationTitle, text, ButtonEnum.Ok, Icon.Error);
    await messageBox.ShowAsync();
  }
}