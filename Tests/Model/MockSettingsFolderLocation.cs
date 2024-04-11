using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockSettingsFolderLocation : ISettingsFolderLocation {
  internal int WriteCount { get; set; }
  public string Path { get; set; } = string.Empty;
  public void Write() {
    WriteCount++;
  }
}