using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockAlertService : IAlertService {
  internal string LastMessage { get; set; } = string.Empty;
  internal string LastTitle { get; set; } = string.Empty;
  internal int ShowAlertCount { get; set; }

  [ExcludeFromCodeCoverage]
  public Task ShowAlertAsync(string title, string message, string cancel = "OK") {
    throw new NotImplementedException();
  }

  [ExcludeFromCodeCoverage]
  public Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes",
    string cancel = "No") {
    throw new NotImplementedException();
  }

  public void ShowAlert(string title, string message, string cancel = "OK") {
    LastTitle = title;
    LastMessage = message;
    ShowAlertCount++;
  }

  [ExcludeFromCodeCoverage]
  public void ShowConfirmation(string title, string message, Action<bool> callback,
    string accept = "Yes", string cancel = "No") {
    throw new NotImplementedException();
  }
}