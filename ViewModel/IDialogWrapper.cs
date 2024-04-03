using System.ComponentModel;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   A service that can show different types of dialog windows.
/// </summary>
public interface IDialogWrapper {
  /// <summary>
  ///   Asynchronously shows an open file dialog, returning the path of the selected file
  ///   or, if the user cancels the dialog, null.
  /// </summary>
  /// <param name="ownerViewModel">Owner view model.</param>
  /// <param name="dialogTitle">Dialog title.</param>
  /// <param name="filterName">Display name of the file type filter.</param>
  /// <param name="fileExtension">
  ///   File name extension of the file type filter. (Must/can? exclude "*.".)
  /// </param>
  Task<string?> BrowseForFileAsync(INotifyPropertyChanged? ownerViewModel,
    string dialogTitle, string filterName, string fileExtension);

  /// <summary>
  ///   Asynchronously shows an open folder dialog, returning the path of the selected
  ///   folder or, if the user cancels the dialog, null.
  /// </summary>
  /// <param name="ownerViewModel">Owner view model.</param>
  /// <param name="dialogTitle">Dialog title.</param>
  Task<string?> BrowseForFolderAsync(INotifyPropertyChanged? ownerViewModel,
    string dialogTitle);

  /// <summary>
  ///   Asynchronously shows an error message box.
  /// </summary>
  /// <param name="ownerViewModel">Owner view model.</param>
  /// <param name="text">The message text to be shown.</param>
  /// <returns></returns>
  Task ShowErrorMessageBoxAsync(INotifyPropertyChanged? ownerViewModel, string text);
}