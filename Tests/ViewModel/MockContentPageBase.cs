using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockContentPageBase : IContentPageBase {
  internal bool ExecuteInvokeAsyncAction { get; set; }
  internal int GoToLocationsPageCount { get; set; }
  internal int DispatchCount { get; set; }

  public void GoToLocationsPage() {
    GoToLocationsPageCount++;
  }

  public void Dispatch(Action action) {
    DispatchCount++;
    if (ExecuteInvokeAsyncAction) {
      action();
    }
  }
}