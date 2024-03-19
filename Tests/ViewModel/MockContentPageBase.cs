using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockContentPageBase : IContentPageBase {
  internal bool ExecuteInvokeAsyncAction { get; set; }
  internal int GoToLocationsPageCount { get; set; }
  internal int InvokeAsyncCount { get; set; }

  public void GoToLocationsPage() {
    GoToLocationsPageCount++;
  }

  public void InvokeAsync(Action action) {
    InvokeAsyncCount++;
    if (ExecuteInvokeAsyncAction) {
      action();
    }
  }
}