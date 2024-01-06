using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.Tests.ViewModel;

public class MockFilePicker : IFilePicker {
  internal bool Cancel { get; set; }
  internal string ExpectedPath { get; set; } = string.Empty;
  // internal int PickAsyncCount { get; set; }

  public async Task<FileResult?> PickAsync(PickOptions? options = null) {
    // PickAsyncCount++;
    await Task.Delay(0);
    var fileResult = Cancel ? null : new FileResult(ExpectedPath); 
    return fileResult;
  }

  [ExcludeFromCodeCoverage]
  public async Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions? options = null) {
    await Task.Delay(0);
    throw new NotImplementedException();
  }
}