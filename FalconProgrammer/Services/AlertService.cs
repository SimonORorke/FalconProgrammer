using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

// Based on ToolmakerSteve's answer in
// https://stackoverflow.com/questions/72429055/how-to-displayalert-in-a-net-maui-viewmodel
public class AlertService : IAlertService {
  private static IAlertService? _default;
  public static IAlertService Default => _default ??= new AlertService();

  // ----- async calls (use with "await" - MUST BE ON DISPATCHER THREAD) -----

  public Task ShowAlertAsync(string title, string message, string cancel = "OK") {
    return Application.Current!.MainPage!.DisplayAlert(title, message, cancel);
  }

  public Task<bool> ShowConfirmationAsync(string title, string message,
    string accept = "Yes", string cancel = "No") {
    return Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel);
  }

  // ----- "Fire and forget" calls -----

  /// <summary>
  ///   "Fire and forget". Method returns BEFORE showing alert.
  /// </summary>
  public void ShowAlert(string title, string message, string cancel = "OK") {
    // ReSharper disable once AsyncVoidLambda
    Application.Current!.MainPage!.Dispatcher.Dispatch(async () =>
      await ShowAlertAsync(title, message, cancel)
    );
  }

  /// <summary>
  ///   "Fire and forget". Method returns BEFORE showing alert.
  /// </summary>
  /// <param name="callback">Action to perform afterwards.</param>
  public void ShowConfirmation(string title, string message, Action<bool> callback,
    string accept = "Yes", string cancel = "No") {
    // ReSharper disable once AsyncVoidLambda
    Application.Current!.MainPage!.Dispatcher.Dispatch(async () => {
      bool answer = await ShowConfirmationAsync(title, message, accept, cancel);
      callback(answer);
    });
  }
}