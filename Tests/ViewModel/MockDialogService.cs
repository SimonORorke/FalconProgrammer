﻿using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockDialogService : IDialogService {
  internal bool Cancel { get; set; }
  internal string ExpectedPath { get; set; } = string.Empty;
  internal string LastErrorMessage { get; set; } = string.Empty;
  internal int ShowErrorMessageBoxCount { get; set; }

  public async Task<string?> BrowseForFileAsync(
    string dialogTitle,
    string filterName, string fileExtension) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }

  public async Task<string?> BrowseForFolderAsync(string dialogTitle) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }

  public async Task ShowErrorMessageBoxAsync(string text) {
    await Task.Delay(0);
    ShowErrorMessageBoxCount++;
    LastErrorMessage = text;
  }
}