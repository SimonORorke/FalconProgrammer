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
        // FileTypeFilter is for Windows only. MacOS open file dialogs, at least the ones
        // I've seen, don't display file type filters. Instead, they have a pattern
        // search box. So AppleUniformTypeIdentifiers, the macOS equivalent of
        // FileTypeFilter, has presumably already applied its filter when the dialog is
        // shown. I don't know whether it's possible to define a uniform type identifier
        // for Falcon programs. Even if it is, it does not seem worth it, as the user is
        // very probably browsing a folder that contains nothing but Falcon programs. 
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