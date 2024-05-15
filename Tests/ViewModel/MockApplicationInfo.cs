using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockApplicationInfo : IApplicationInfo {
  // public string Company { get; } = "Simon O\'Rorke";
  public string Copyright => "Copyright \u00a9 2024 Simon O\'Rorke";
  public string Product => "Falcon Programmer";
  public string Version => "99.99.99";
}