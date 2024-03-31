using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class MockContentPageBase : IContentPageBase {
  [PublicAPI] internal int GoToLocationsPageCount { get; set; }

  public void GoToLocationsPage() {
    GoToLocationsPageCount++;
  }
}