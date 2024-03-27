using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockFolderChooser : IFolderChooser {
  internal bool Cancel { get; set; }
  internal string ExpectedPath { get; set; } = string.Empty;

  // public async Task<FolderPickerResult> PickAsync(
  //   CancellationToken cancellationToken = new CancellationToken()) {
  //   Settings.Folder? folder;
  //   Exception? exception;
  //   if (Cancel) {
  //     // Set the folder and exception such that FolderPickerResult.IsSuccessful will be
  //     // set to false.
  //     folder = null;
  //     exception = new OperationCanceledException();
  //   } else {
  //     // Set the folder and exception such that FolderPickerResult.IsSuccessful will be
  //     // set to true.
  //     folder = !string.IsNullOrWhiteSpace(ExpectedPath)
  //       ? new Settings.Folder(ExpectedPath, Path.GetFileName(ExpectedPath))
  //       : throw new InvalidOperationException(
  //         "MockFolderChooser.ExpectedPath has not been specified.");
  //     exception = null;
  //   }
  //   var folderPickerResult = new FolderPickerResult(folder, exception);
  //   await Task.Delay(0, cancellationToken);
  //   return folderPickerResult;
  // }

  public async Task<string?> ChooseAsync() {
    if (Cancel) {
      return null;
    }
    await Task.Delay(0);
    return ExpectedPath;
  }
}