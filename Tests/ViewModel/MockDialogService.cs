using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class MockDialogService : IDialogService {
  [PublicAPI] internal int AskYesNoQuestionCount { get; set; }
  internal bool Cancel { get; set; }
  [PublicAPI] internal string LastErrorMessage { get; set; } = string.Empty;
  [PublicAPI] internal string LastMessageWindowMessage { get; set; } = string.Empty;
  [PublicAPI] internal string LastMessageWindowTitle { get; set; } = string.Empty;
  [PublicAPI] internal bool LastYesNoAnswer { get; set; }
  [PublicAPI] internal string LastYesNoQuestion { get; set; } = string.Empty;
  [PublicAPI] internal int ShowAboutBoxCount { get; set; }
  [PublicAPI] internal int ShowColourSchemeDialogCount { get; set; }
  [PublicAPI] internal int ShowErrorMessageBoxCount { get; set; }
  [PublicAPI] internal int ShowMessageWindowCount { get; set; }
  internal string SimulatedPath { get; set; } = string.Empty;
  internal bool SimulatedYesNoAnswer { get; set; }

  public async Task<bool> AskYesNoQuestion(string text) {
    await Task.Delay(0);
    AskYesNoQuestionCount++;
    LastYesNoQuestion = text;
    return LastYesNoAnswer = SimulatedYesNoAnswer;
  }

  public async Task<string?> BrowseForFolder(string dialogTitle) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return SimulatedPath;
  }

  public async Task<string?> OpenFile(
    string dialogTitle,
    string filterName, string fileExtension) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return SimulatedPath;
  }

  public async Task<string?> SaveFile(
    string dialogTitle, string filterName, string fileExtension) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return SimulatedPath;
  }

  public async Task ShowAboutBox(AboutWindowViewModel viewModel) {
    await Task.Delay(0);
    ShowAboutBoxCount++;
  }

  public async Task ShowColourSchemeDialog(ColourSchemeWindowViewModel viewModel) {
    await Task.Delay(0);
    ShowColourSchemeDialogCount++;
  }

  public async Task ShowErrorMessageBox(string text, string tabTitle = "") {
    await Task.Delay(0);
    ShowErrorMessageBoxCount++;
    LastErrorMessage = text;
  }

  public async Task ShowMessageWindow(MessageWindowViewModel viewModel) {
    await Task.Delay(0);
    ShowMessageWindowCount++;
    LastMessageWindowMessage = viewModel.Text;
    LastMessageWindowTitle = viewModel.Title;
  }
}