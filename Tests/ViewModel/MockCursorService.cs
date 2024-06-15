using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class MockCursorService : ICursorService {
  [PublicAPI] internal int ShowDefaultCursorCount { get; set; }
  [PublicAPI] internal int ShowWaitCursorCount { get; set; }

  public void ShowDefaultCursor() {
    ShowDefaultCursorCount++;
  }

  public void ShowWaitCursor() {
    ShowWaitCursorCount++;
  }
}