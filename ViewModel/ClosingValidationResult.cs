namespace FalconProgrammer.ViewModel;

/// <summary>
///   For use to report the result of validation that has been performed when the user
///   attempts to close the current tab page, either by selecting another tab or by
///   closing the main window.
/// </summary>
public class ClosingValidationResult {
  /// <summary>
  ///   For use to report the result of validation that has been performed when the user
  ///   attempts to close the current tab page, either by selecting another tab or by
  ///   closing the main window.
  /// </summary>
  /// <param name="success">Indicates whether the validation has been successful.</param>
  /// <param name="canClosePage">
  ///   Indicates whether the page can be closed. Should be false only if the validation
  ///   failed on closing the main window and the user then cancelled closing the window
  ///   when the error was shown.
  /// </param>
  public ClosingValidationResult(bool success, bool canClosePage) {
    Success = success;
    CanClosePage = canClosePage;
  }

  /// <summary>Indicates whether the validation has been successful.</summary>
  public bool Success { get; }

  /// <summary>
  ///   Indicates whether the page can be closed. Should be false only if the validation
  ///   failed on closing the main window and the user then cancelled closing the window
  ///   when the error was shown.
  /// </summary>
  public bool CanClosePage { get; }
}