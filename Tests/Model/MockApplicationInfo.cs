using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockApplicationInfo : IApplicationInfo {
  // public string Company { get; } = "Simon O\'Rorke";
  public string Copyright { get; set; } = "Copyright \u00a9 2024 Simon O\'Rorke";
  public string Product { get; set; } = "Falcon Programmer";
  public string Version { get; set; } = "99.99.99";
}