using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

// Based on ToolmakerSteve's answer in
// https://stackoverflow.com/questions/72429055/how-to-displayalert-in-a-net-maui-viewmodel
public class AlertService : IAlertService {
  private static IAlertService? _default;
  private AlertService() { }
  public static IAlertService Default => _default ??= new AlertService();

  /// <summary>
  ///   Displays an alert dialog to the application user with a single cancel button.
  ///   Async calls (use with "await" - MUST BE ON DISPATCHER THREAD).
  /// </summary>
  /// <param name="title">The title of the alert dialog.</param>
  /// <param name="message">The body text of the alert dialog.</param>
  /// <param name="cancel">Text to be displayed on the 'Cancel' button.</param>
  public Task ShowAlertAsync(string title, string message, string cancel = "OK") {
    // Debug.WriteLine($"AlertService.ShowAlertAsync: {message}");
    return Application.Current!.MainPage!.DisplayAlert(title, message, cancel);
  }

  /// <summary>
  ///   Presents an alert dialog to the application user with an accept and a cancel
  ///   button.
  ///   Async calls (use with "await" - MUST BE ON DISPATCHER THREAD).
  /// </summary>
  /// <param name="title">The title of the alert dialog.</param>
  /// <param name="message">The body text of the alert dialog.</param>
  /// <param name="accept">Text to be displayed on the 'Accept' button.</param>
  /// <param name="cancel">Text to be displayed on the 'Cancel' button.</param>
  public Task<bool> ShowConfirmationAsync(string title, string message,
    string accept = "Yes", string cancel = "No") {
    return Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel);
  }

  /// <summary>
  ///   Displays an alert dialog to the application user with a single cancel button.
  ///   "Fire and forget". Method returns BEFORE showing alert.
  /// </summary>
  /// <param name="title">The title of the alert dialog.</param>
  /// <param name="message">The body text of the alert dialog.</param>
  /// <param name="cancel">Text to be displayed on the 'Cancel' button.</param>
  public void ShowAlert(string title, string message, string cancel = "OK") {
    // ReSharper disable once AsyncVoidLambda
    Application.Current!.MainPage!.Dispatcher.Dispatch(async () =>
      await ShowAlertAsync(title, message, cancel)
    );
  }

  /// <summary>
  ///   Presents an alert dialog to the application user with an accept and a cancel
  ///   button, specifying a callback action to respond to the result.
  ///   "Fire and forget". Method returns BEFORE showing alert.
  /// </summary>
  /// <param name="title">The title of the alert dialog.</param>
  /// <param name="message">The body text of the alert dialog.</param>
  /// <param name="callback">Action to perform afterwards.</param>
  /// <param name="accept">Text to be displayed on the 'Accept' button.</param>
  /// <param name="cancel">Text to be displayed on the 'Cancel' button.</param>
  public void ShowConfirmation(string title, string message, Action<bool> callback,
    string accept = "Yes", string cancel = "No") {
    // ReSharper disable once AsyncVoidLambda
    Application.Current!.MainPage!.Dispatcher.Dispatch(async () => {
      bool answer = await ShowConfirmationAsync(title, message, accept, cancel);
      callback(answer);
    });
  }
}