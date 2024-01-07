using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;

namespace FalconProgrammer.Tests.ViewModel;

public class MockFolderPicker : IFolderPicker {
  internal bool Cancel { get; set; }
  internal string ExpectedPath { get; set; } = string.Empty;
  
  [ExcludeFromCodeCoverage]
  public async Task<FolderPickerResult> PickAsync(string initialPath,
    CancellationToken cancellationToken = new CancellationToken()) {
    await Task.Delay(0, cancellationToken);
    throw new NotImplementedException();
  }

  public async Task<FolderPickerResult> PickAsync(
    CancellationToken cancellationToken = new CancellationToken()) {
    Folder? folder;
    Exception? exception;
    if (Cancel) {
      // Set the folder and exception such that FolderPickerResult.IsSuccessful will be
      // set to false.
      folder = null;
      exception = new OperationCanceledException();
    } else {
      // Set the folder and exception such that FolderPickerResult.IsSuccessful will be
      // set to true.
      folder = !string.IsNullOrWhiteSpace(ExpectedPath)
        ? new Folder(ExpectedPath, Path.GetFileName(ExpectedPath))
        : throw new InvalidOperationException(
          "MockFolderPicker.ExpectedPath has not been specified.");
      exception = null;
    }
    var folderPickerResult = new FolderPickerResult(folder, exception);
    await Task.Delay(0, cancellationToken);
    return folderPickerResult;
  }
}