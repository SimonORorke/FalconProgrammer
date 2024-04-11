namespace FalconProgrammer.ViewModel;

/// <summary>
///   A service that can show different types of dialog windows.
/// </summary>
public interface IDialogService {
  
  /// <summary>
  ///   Asynchronously shows a question message box with Yes and No buttons,
  ///   returning whether Yes has been clicked.
  /// </summary>
  /// <param name="text">The message text to be shown.</param>
  /// <returns>True if Yes, false if No.</returns>
  Task<bool> AskYesNoQuestionAsync(string text);
  
  /// <summary>
  ///   Asynchronously shows an open file dialog, returning the path of the selected file
  ///   or, if the user cancels the dialog, null.
  /// </summary>
  /// <param name="dialogTitle">Dialog title.</param>
  /// <param name="filterName">Display name of the file type filter.</param>
  /// <param name="fileExtension">
  ///   File name extension of the file type filter, excluding "*.".)
  /// </param>
  Task<string?> BrowseForFileAsync(
    string dialogTitle, string filterName, string fileExtension);

  /// <summary>
  ///   Asynchronously shows an open folder dialog, returning the path of the selected
  ///   folder or, if the user cancels the dialog, null.
  /// </summary>
  /// <param name="dialogTitle">Dialog title.</param>
  Task<string?> BrowseForFolderAsync(string dialogTitle);

  /// <summary>
  ///   Asynchronously shows an error message box.
  /// </summary>
  /// <param name="text">The message text to be shown.</param>
  /// <returns></returns>
  Task ShowErrorMessageBoxAsync(string text);
}