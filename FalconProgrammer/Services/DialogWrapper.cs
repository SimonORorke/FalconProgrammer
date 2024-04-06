using System.ComponentModel;
using System.Threading.Tasks;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

/// <summary>
///   A service that can show different types of dialog windows.
/// </summary>
public class DialogWrapper : IDialogWrapper {
  // public DialogWrapper(ILoggerFactory loggerFactory) {
  //   var customDialogManager = new CustomDialogManager(
  //     new ViewLocator(),
  //     new DialogFactory().AddMessageBox(),
  //     loggerFactory.CreateLogger<DialogManager>());
  //   DialogService = new DialogService(
  //     customDialogManager,
  //     x => Locator.Current.GetService(x));
  // }
  //
  // internal IDialogService DialogService { get; }

  public async Task<string?> BrowseForFileAsync(
    INotifyPropertyChanged? ownerViewModel, string dialogTitle, string filterName,
    string fileExtension) {
    await Task.Delay(0);
    return null;
    // var result = await DialogService.ShowOpenFileDialogAsync(
    //   ownerViewModel,
    //   new OpenFileDialogSettings {
    //     Title = dialogTitle,
    //     Filters = new List<FileFilter> {
    //       new FileFilter(filterName, new[] { fileExtension })
    //     }
    //   });
    // return result?.Path.LocalPath;
  }

  public async Task<string?> BrowseForFolderAsync(
    INotifyPropertyChanged? ownerViewModel, string dialogTitle) {
    await Task.Delay(0);
    return null;
    // var result = await DialogService.ShowOpenFolderDialogAsync(
    //   ownerViewModel,
    //   new OpenFolderDialogSettings { Title = dialogTitle });
    // return result?.Path.LocalPath;
  }

  public async Task ShowErrorMessageBoxAsync(
    INotifyPropertyChanged? ownerViewModel, string text) {
    await Task.Delay(0);
    // await DialogService.ShowMessageBoxAsync(ownerViewModel, text,
    //   Application.Current!.Name!,
    //   MessageBoxButton.Ok,
    //   MessageBoxImage.Error);
  }
}