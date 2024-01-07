using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.Tests.ViewModel;

public class MockFilePicker : IFilePicker {
  internal bool Cancel { get; set; }
  internal string ExpectedPath { get; set; } = string.Empty;

  public async Task<FileResult?> PickAsync(PickOptions? options = null) {
    await Task.Delay(0);
    var fileResult = Cancel 
      ? null 
      : !string.IsNullOrWhiteSpace(ExpectedPath) 
        ? new FileResult(ExpectedPath)
        : throw new InvalidOperationException(
          "MockFilePicker.ExpectedPath has not been specified."); 
    return fileResult;
  }

  [ExcludeFromCodeCoverage]
  public async Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions? options = null) {
    await Task.Delay(0);
    throw new NotImplementedException();
  }
}