using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockApplicationInfo : IApplicationInfo {
  public string Company { get; } = "Simon O\'Rorke";
  public string Copyright { get; } = "Copyright \u00a9 2024 Simon O\'Rorke";
  public string Product { get; } = "Falcon Programmer";
  public string Version { get; } = "99.99.99";
}