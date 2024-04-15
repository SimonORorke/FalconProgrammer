using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   For view model tests.
/// </summary>
public class MockSettingsFolderLocationReader : TestSettingsFolderLocationReader {
  public override ISettingsFolderLocation Read() {
    var location = TestDeserialiser.Deserialise("Will be ignored");
    return new MockSettingsFolderLocation {
      Path = location.Path
    };
  }
}