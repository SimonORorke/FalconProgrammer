using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class MockContentPageBase : IContentPageBase {
  [PublicAPI] internal int GoToLocationsPageCount { get; set; }
  [PublicAPI] internal int DispatchCount { get; set; }

  public void GoToLocationsPage() {
    GoToLocationsPageCount++;
  }

  public void Dispatch(Action action) {
    DispatchCount++;
    action();
  }
}