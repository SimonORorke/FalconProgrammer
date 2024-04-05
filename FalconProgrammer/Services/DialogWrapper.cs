using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using FalconProgrammer.ViewModel;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using Splat;

namespace FalconProgrammer.Services;

/// <summary>
///   A service that can show different types of dialog windows. Many more types could
///   easily be added: for what is possible, see
///   https://github.com/mysteryx93/HanumanInstitute.MvvmDialogs.
/// </summary>
/// <remarks>
///   This wrapper for <see cref="DialogService" /> is needed in order to allow mocking
///   for unit tests. For ease of use, <see cref="DialogService" /> extension methods
///   are called. But mocking extension methods requires fake assemblies provided by
///   Microsoft.Fakes, which is only available with Visual Studio Enterprise Edition, and
///   can be used in Jetbrains Rider if Visual Studio Enterprise Edition is installed. See
///   https://youtrack.jetbrains.com/issue/RIDER-97620/unit-tests-failed-when-using-microsoft-fakes.
///   <para>
///     To be fair, HanumanInstitute does explain a way to mock
///     <see cref="DialogService" /> directly, using <see cref="IDialogService" />. See
///     https://github.com/mysteryx93/HanumanInstitute.MvvmDialogs?tab=readme-ov-file#unit-testing.
///     However, I still think the use of a wrapper has benefits for this application,
///     as the application's dialog requirements are simple, and the wrapper approach
///     hides unneeded complexity. 
///   </para>
/// </remarks>
public class DialogWrapper : IDialogWrapper {
  public DialogWrapper(ILoggerFactory loggerFactory) {
    var dialogManager = new DialogManager(
      new ViewLocator(),
      new DialogFactory().AddMessageBox(),
      loggerFactory.CreateLogger<DialogManager>());
    DialogService = new DialogService(
      dialogManager,
      x => Locator.Current.GetService(x));
  }

  internal IDialogService DialogService { get; }

  public async Task<string?> BrowseForFileAsync(
    INotifyPropertyChanged? ownerViewModel, string dialogTitle, string filterName,
    string fileExtension) {
    var result = await DialogService.ShowOpenFileDialogAsync(
      ownerViewModel,
      new OpenFileDialogSettings {
        Title = dialogTitle,
        Filters = new List<FileFilter> {
          new FileFilter(filterName, new[] { fileExtension })
        }
      });
    return result?.Path.LocalPath;
  }

  public async Task<string?> BrowseForFolderAsync(
    INotifyPropertyChanged? ownerViewModel, string dialogTitle) {
    var result = await DialogService.ShowOpenFolderDialogAsync(
      ownerViewModel,
      new OpenFolderDialogSettings { Title = dialogTitle });
    return result?.Path.LocalPath;
  }

  public async Task ShowErrorMessageBoxAsync(
    INotifyPropertyChanged? ownerViewModel, string text) {
    await DialogService.ShowMessageBoxAsync(ownerViewModel, text,
      Application.Current!.Name!,
      MessageBoxButton.Ok,
      MessageBoxImage.Error);
  }
}