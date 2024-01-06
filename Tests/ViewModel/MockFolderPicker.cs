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
    var folder = Cancel 
      ? null 
      : new Folder(ExpectedPath, Path.GetFileName(ExpectedPath));
    var folderPickerResult = new FolderPickerResult(folder, null);
    await Task.Delay(0, cancellationToken);
    return folderPickerResult;
  }
}