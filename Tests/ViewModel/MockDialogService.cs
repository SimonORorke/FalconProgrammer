using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class MockDialogService : IDialogService {
  [PublicAPI] internal int AskYesNoQuestionCount { get; set; }
  internal bool Cancel { get; set; }
  internal string ExpectedPath { get; set; } = string.Empty;
  internal bool ExpectedYesNoAnswer { get; set; }
  [PublicAPI] internal string LastErrorMessage { get; set; } = string.Empty;
  [PublicAPI] internal bool LastYesNoAnswer { get; set; }
  [PublicAPI] internal int ShowErrorMessageBoxCount { get; set; }

  public async Task<bool> AskYesNoQuestion(string text) {
    await Task.Delay(0);
    AskYesNoQuestionCount++;
    return LastYesNoAnswer = ExpectedYesNoAnswer;
  }

  public async Task<string?> BrowseForFolder(string dialogTitle) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }

  public async Task<string?> OpenFile(
    string dialogTitle,
    string filterName, string fileExtension) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }

  public async Task<string?> SaveFile(
    string dialogTitle, string defaultExtension) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }

  public async Task ShowErrorMessageBox(string text) {
    await Task.Delay(0);
    ShowErrorMessageBoxCount++;
    LastErrorMessage = text;
  }
}