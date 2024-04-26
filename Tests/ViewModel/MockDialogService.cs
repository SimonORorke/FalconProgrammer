using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockDialogService : IDialogService {
  internal int AskYesNoQuestionCount { get; set; }
  internal bool Cancel { get; set; }
  internal string ExpectedPath { get; set; } = string.Empty;
  internal bool ExpectedYesNoAnswer { get; set; }
  internal string LastErrorMessage { get; set; } = string.Empty;
  internal bool LastYesNoAnswer { get; set; }
  internal int ShowErrorMessageBoxCount { get; set; }

  public async Task<bool> AskYesNoQuestion(string text) {
    await Task.Delay(0);
    AskYesNoQuestionCount++;
    return LastYesNoAnswer = ExpectedYesNoAnswer;
  }

  public async Task<string?> BrowseForFile(
    string dialogTitle,
    string filterName, string fileExtension) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }

  public async Task<string?> BrowseForFolder(string dialogTitle) {
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