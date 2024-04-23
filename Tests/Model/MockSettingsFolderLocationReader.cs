using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   For view model tests. Use <see cref="TestSettingsFolderLocationReader" /> for model
///   tests.
/// </summary>
public class MockSettingsFolderLocationReader : TestSettingsFolderLocationReader {
  internal bool ExpectedFileExists { get; set; } = true;

  public override ISettingsFolderLocation Read() {
    var result = new MockSettingsFolderLocation();
    if (ExpectedFileExists) {
      var location = TestDeserialiser.Deserialise("Will be ignored");
      result.Path = location.Path;
    }
    return result;
  }
}