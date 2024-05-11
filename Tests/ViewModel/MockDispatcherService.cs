using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class MockDispatcherService : IDispatcherService {
  [PublicAPI] internal int DispatchCount { get; set; }

  public void Dispatch(Action action) {
    DispatchCount++;
    action();
  }
}