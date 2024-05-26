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
  /// <param name="tabTitle">
  ///   Optionally specifies a tab title to be appended to the message box title.
  /// </param>
  /// <returns>True if Yes, false if No.</returns>
  Task<bool> AskYesNoQuestion(string text, string tabTitle = "");

  /// <summary>
  ///   Asynchronously shows an open folder dialog, returning the path of the selected
  ///   folder or, if the user cancels the dialog, null.
  /// </summary>
  /// <param name="dialogTitle">Dialog title.</param>
  Task<string?> BrowseForFolder(string dialogTitle);

  /// <summary>
  ///   Asynchronously shows an open file dialog, returning the path of the selected file
  ///   or, if the user cancels the dialog, null.
  /// </summary>
  /// <param name="dialogTitle">Dialog title.</param>
  /// <param name="filterName">Display name of the file type filter.</param>
  /// <param name="fileExtension">
  ///   File name extension of the file type filter, excluding "*.".)
  /// </param>
  Task<string?> OpenFile(
    string dialogTitle, string filterName, string fileExtension);

  /// <summary>
  ///   Asynchronously shows a save file dialog, returning the path of the specified file
  ///   or, if the user cancels the dialog, null.
  /// </summary>
  /// <param name="dialogTitle">Dialog title.</param>
  /// <param name="filterName">Display name of the file type filter.</param>
  /// <param name="fileExtension">
  ///   Default file name extension and file name extension of the file type filter,
  ///   excluding ".".)
  /// </param>
  Task<string?> SaveFile(
    string dialogTitle, string filterName, string fileExtension);

  /// <summary>
  ///   Asynchronously shows the About box.
  /// </summary>
  /// <param name="viewModel">
  ///   The view model to be assigned to the About box.
  /// </param>
  Task ShowAboutBox(AboutWindowViewModel viewModel);

  /// <summary>
  ///   Asynchronously shows the Colour Scheme dialog box.
  /// </summary>
  /// <param name="viewModel">
  ///   The view model to be assigned to the Colour Scheme dialog box.
  /// </param>
  Task ShowColourSchemeDialog(ColourSchemeWindowViewModel viewModel);

  /// <summary>
  ///   Asynchronously shows an error message box.
  /// </summary>
  /// <param name="text">The message text to be shown.</param>
  /// <param name="tabTitle">
  ///   Optionally specifies a tab title to be appended to the message box title.
  /// </param>
  Task ShowErrorMessageBox(string text, string tabTitle = "");

  /// <summary>
  ///   Asynchronously shows the Message window.
  /// </summary>
  /// <param name="viewModel">
  ///   The view model to be assigned to the Message window.
  /// </param>
  Task ShowMessageWindow(MessageWindowViewModel viewModel);
}