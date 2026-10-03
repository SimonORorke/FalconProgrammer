using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using FalconProgrammer.ViewModel;
using FalconProgrammer.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace FalconProgrammer.Services;

/// <summary>
///   A service that can show different types of dialog windows.
/// </summary>
public class DialogService : IDialogService {
  private string ApplicationTitle => field ??= Application.Current!.Name!;
  private Window? CurrentDialog { get; set; }
  private Window MainWindow => field ??= ((App)Application.Current!).MainWindow;

  public async Task<bool> AskYesNoQuestion(string text, string tabTitle = "") {
    // A wait cursor is shown when changing pages. A message box may be shown
    // when the new page is shown, and before the default cursor is shown.
    // So we need to show the default cursor before showing the message box.
    MainWindow.Cursor = Cursor.Default;
    var messageBox = MessageBoxManager.GetMessageBoxStandard(
      GetMessageBoxTitle(tabTitle), text, ButtonEnum.YesNo, Icon.Question);
    var result = await messageBox.ShowAsync();
    return result == ButtonResult.Yes;
  }

  public async Task<string?> BrowseForFolder(string dialogTitle) {
    var folders =
      await MainWindow.StorageProvider.OpenFolderPickerAsync(
        new FolderPickerOpenOptions {
          Title = dialogTitle,
          AllowMultiple = false
        }
      );
    return folders.Count == 1
      // On macOS, the folder picker dialog returns a path with a trailing slash.
      // Remove the trailing slash if present.
      ? System.IO.Path.TrimEndingDirectorySeparator(folders[0].Path.LocalPath)
      : null;
  }

  public async Task<string?> OpenFile(
    string dialogTitle, string filterName, string fileExtension) {
    var files = await MainWindow.StorageProvider.OpenFilePickerAsync(
      new FilePickerOpenOptions {
        Title = dialogTitle,
        AllowMultiple = false,
        // FileTypeFilter is for Windows only. MacOS open file dialogs, at least the ones
        // I've seen, don't display file type filters. Instead, they have a pattern
        // search box.
        //
        // So I think FilePickerOpenOptions.AppleUniformTypeIdentifiers must instead be 
        // used for the "Kind" sort option on macOS open file dialogs. I don't know
        // whether it's possible to define a uniform type identifier for Falcon programs.
        // Even if it is, there would seem to be little point in implementing it, as the
        // user is very probably browsing a folder that contains nothing but Falcon
        // programs. 
        FileTypeFilter = [
          new FilePickerFileType(filterName) {
            Patterns = [$"*.{fileExtension}"]
          }
        ]
      });
    return files.Count == 1 ? files[0].Path.LocalPath : null;
  }

  public async Task<string?> SaveFile(
    string dialogTitle, string filterName, string fileExtension) {
    var file = await MainWindow.StorageProvider.SaveFilePickerAsync(
      new FilePickerSaveOptions {
        Title = dialogTitle,
        DefaultExtension = fileExtension,
        ShowOverwritePrompt = true,
        FileTypeChoices = [
          new FilePickerFileType(filterName) {
            Patterns = [$"*.{fileExtension}"]
          }
        ]
      });
    return file?.Path.LocalPath;
  }

  public async Task ShowAboutBox(AboutWindowViewModel viewModel) {
    CurrentDialog = new AboutWindow {
      DataContext = viewModel
    };
    await CurrentDialog.ShowDialog(MainWindow);
    CurrentDialog = null;
  }

  public async Task ShowColourSchemeDialog(ColourSchemeWindowViewModel viewModel) {
    CurrentDialog = new ColourSchemeWindow {
      DataContext = viewModel
    };
    await CurrentDialog.ShowDialog(MainWindow);
    CurrentDialog = null;
  }

  public async Task ShowErrorMessageBox(string text, string tabTitle = "") {
    // A wait cursor is shown when changing pages. A message box may be shown
    // when the new page is shown, and before the default cursor is shown.
    // So we need to show the default cursor before showing the message box.
    MainWindow.Cursor = Cursor.Default;
    var messageBox = MessageBoxManager.GetMessageBoxStandard(
      GetMessageBoxTitle(tabTitle), text, ButtonEnum.Ok, Icon.Error);
    await messageBox.ShowAsync();
  }

  public async Task ShowMessageWindow(MessageWindowViewModel viewModel) {
    var messageWindow = new MessageWindow {
      DataContext = viewModel
    };
    try {
      await messageWindow.ShowDialog(CurrentDialog ?? MainWindow);
    } catch {
      messageWindow.Show();
    }
  }

  public async Task ShowInfoMessageBox(string text, string tabTitle = "") {
    // Console.WriteLine($"[FalconProgrammer] DialogService.ShowInfoMessageBox: text='{text}', tabTitle='{tabTitle}'");
    MainWindow.Cursor = Cursor.Default;
    var messageBox = MessageBoxManager.GetMessageBoxStandard(
      GetMessageBoxTitle(tabTitle), text, ButtonEnum.Ok, Icon.Info);
    await messageBox.ShowAsync();
  }

  public async Task ShowNewVersionWindow(NewVersionWindowViewModel viewModel) {
    // Console.WriteLine($"[FalconProgrammer] DialogService.ShowNewVersionWindow: Starting for version '{viewModel.LatestVersion}', MainWindow.IsVisible={MainWindow.IsVisible}, MainWindow.IsLoaded={MainWindow.IsLoaded}");
    var dialog = new NewVersionWindow {
      DataContext = viewModel
    };
    CurrentDialog = dialog;
    // Console.WriteLine("[FalconProgrammer] DialogService.ShowNewVersionWindow: Calling dialog.ShowDialog(MainWindow)...");
    try {
      await dialog.ShowDialog(MainWindow);
      // Console.WriteLine("[FalconProgrammer] DialogService.ShowNewVersionWindow: dialog.ShowDialog completed.");
    } catch {
      // Console.WriteLine("[FalconProgrammer] DialogService.ShowNewVersionWindow: ShowDialog failed. Falling back to dialog.Show().");
      dialog.Show();
    }
    CurrentDialog = null;
  }

  private string GetMessageBoxTitle(string tabTitle) {
    return tabTitle == string.Empty
      ? ApplicationTitle
      : $"{ApplicationTitle} - {tabTitle}";
  }
}