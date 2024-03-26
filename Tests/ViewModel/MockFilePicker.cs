using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockFilePicker : IFileChooser {
  internal bool Cancel { get; set; }
  internal string ExpectedPath { get; set; } = string.Empty;

  // public async Task<FileResult?> PickAsync(PickOptions? options = null) {
  //   await Task.Delay(0);
  //   var fileResult = Cancel 
  //     ? null 
  //     : !string.IsNullOrWhiteSpace(ExpectedPath) 
  //       ? new FileResult(ExpectedPath)
  //       : throw new InvalidOperationException(
  //         "MockFilePicker.ExpectedPath has not been specified."); 
  //   return fileResult;
  // }
  public async Task<string?> ChooseAsync(string title, string fileType) {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }
}