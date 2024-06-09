using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

internal class ErrorReporter {
  public ErrorReporter(IDialogService dialogService) {
    DialogService = dialogService;
  }

  private IDialogService DialogService { get; }

  /// <summary>
  ///   For use when there is an error message to show when the user attempts to close
  ///   current tab page, either by selecting another tab or by closing the main window.
  ///   If the window is closing, the error message is shown in a question message box
  ///   asking whether the window should still be closed. Otherwise the error message
  ///   is shown in a message box with OK button only.
  /// </summary>
  /// <param name="errorMessage">The error message to show.</param>
  /// <param name="tabTitle">
  ///   The tab title to be appended to the message box title.
  /// </param>
  /// <param name="isClosingWindow">
  ///   True if the window is closing, false if another tab has been selected.
  /// </param>
  /// <param name="askOnChangingTabs">
  ///   If another tab has been selected, specifies whether the user is to be asked
  ///   to confirm the tab change. If false, the current tab will be reopened.
  /// </param>
  /// <returns>
  ///   True if the user has been asked whether the window is to be closed and answered
  ///   yes, otherwise false.
  /// </returns>
  /// <remarks>
  ///   Assumptions:
  ///   If the main window is being closed, the user can opt to close the window anyway
  ///   or resume to fix the error.
  ///   When another tab is selected, the user may to be forced to return
  ///   to the current tab to fix the error or given the option, depending on
  ///   <paramref name="askOnChangingTabs " />;
  /// </remarks>
  public async Task<bool> CanClosePageOnError(
    string errorMessage, string tabTitle, bool isClosingWindow, bool askOnChangingTabs) {
    if (isClosingWindow) {
      errorMessage +=
        $"\r\n\r\nAnswer Yes (Enter) to close {Global.ApplicationName}, " +
        "No (Esc) to resume.";
      return await DialogService.AskYesNoQuestion(errorMessage, tabTitle);
    }
    if (askOnChangingTabs) {
      errorMessage +=
        $"\r\n\r\nAnswer Yes (Enter) to close the {tabTitle} page and lose all " +
        $"changes, No (Esc) to return to the {tabTitle} page to fix errors.";
      return await DialogService.AskYesNoQuestion(errorMessage, tabTitle);
    }
    await DialogService.ShowErrorMessageBox(errorMessage, tabTitle);
    return false;
  }
}