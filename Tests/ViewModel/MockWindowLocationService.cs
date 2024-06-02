using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockWindowLocationService : IWindowLocationService {
  public int? Left { get; set; }
  public int? Top { get; set; }
  public int? Width { get; set; }
  public int? Height { get; set; }
  public int? WindowState { get; set; }

  [ExcludeFromCodeCoverage]
  public void Restore() {
    throw new NotImplementedException();
  }

  [ExcludeFromCodeCoverage]
  public void Update() {
    throw new NotImplementedException();
  }
}