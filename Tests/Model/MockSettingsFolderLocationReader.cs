using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   For view model tests. Use <see cref="TestSettingsFolderLocationReader" /> for model
///   tests.
/// </summary>
public class MockSettingsFolderLocationReader : TestSettingsFolderLocationReader {
  internal bool SimulatedFileExists { get; set; } = true;

  public override ISettingsFolderLocation Read() {
    var result = new MockSettingsFolderLocation();
    if (SimulatedFileExists) {
      var location = TestDeserialiser.Deserialise("Will be ignored");
      result.Path = location.Path;
    }
    return result;
  }
}