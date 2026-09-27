using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A test Settings reader that reads embedded files, but with a mock settings folder
///   location reader.
///   For view model tests. Use <see cref="TestSettingsReaderEmbedded" /> for model tests.
/// </summary>
public class MockSettingsReaderEmbedded : TestSettingsReaderEmbedded {
  internal MockSettingsFolderLocationReader MockSettingsFolderLocationReader {
    get => field ??= new MockSettingsFolderLocationReader();
    set;
  }

  protected override SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return MockSettingsFolderLocationReader;
  }
}