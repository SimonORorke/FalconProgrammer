using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class TestSettings : Settings {
  internal int SerializeCount { get; set; }

  protected override void Serialize() {
    SerializeCount++;
  }
}