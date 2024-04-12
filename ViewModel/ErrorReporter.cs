using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

internal class ErrorReporter(IDialogService dialogService) {
  
  /// <summary>
  ///   If the user is attempting to close the window, shows the error message in a
  ///   question message box asking whether the window should still be closed.
  ///   Otherwise shows error message in a message box with OK button only. 
  /// </summary>
  /// <param name="errorMessage">The error message to show.</param>
  /// <param name="isClosingWindow">
  ///   Whether the error has been detected while the user is attempting to close the
  ///   window.
  /// </param>
  /// <returns>
  ///   True if the user has been asked whether the window is to be closed and answered
  ///   yes, otherwise false.
  /// </returns>
  public async Task<bool> CanCloseWindowOnErrorAsync(
    string errorMessage, bool isClosingWindow) {
    if (isClosingWindow) {
      errorMessage +=
        $"\r\n\r\nAnswer Yes (Enter) to close {Global.ApplicationTitle}, " +
        "No (Esc) to resume.";
      return await dialogService.AskYesNoQuestionAsync(errorMessage);
    }
    await dialogService.ShowErrorMessageBoxAsync(errorMessage);
    return false;
  }
}