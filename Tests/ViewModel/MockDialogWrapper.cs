using System.ComponentModel;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockDialogWrapper : IDialogWrapper {
  internal bool Cancel { get; set; }
  internal string ExpectedPath { get; set; } = string.Empty;
  internal string LastErrorMessage { get; set; } = string.Empty;
  internal int ShowErrorMessageBoxCount { get; set; }

  public async Task<string?> BrowseForFileAsync(INotifyPropertyChanged? ownerViewModel,
    string dialogTitle,
    string filterName, string fileExtension) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }

  public async Task<string?> BrowseForFolderAsync(INotifyPropertyChanged? ownerViewModel,
    string dialogTitle) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }

  public async Task ShowErrorMessageBoxAsync(INotifyPropertyChanged? ownerViewModel,
    string text) {
    await Task.Delay(0);
    ShowErrorMessageBoxCount++;
    LastErrorMessage = text;
  }
}