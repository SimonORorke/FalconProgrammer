using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A test Settings reader that reads embedded files, but with a mock settings folder
///   location reader.
///   For view model tests. Use <see cref="TestSettingsReaderEmbedded" /> for model tests.
/// </summary>
public class MockSettingsReaderEmbedded : TestSettingsReaderEmbedded {
  private MockSettingsFolderLocationReader? _mockSettingsFolderLocationReader;

  internal MockSettingsFolderLocationReader MockSettingsFolderLocationReader {
    get => _mockSettingsFolderLocationReader ??= new MockSettingsFolderLocationReader();
    set => _mockSettingsFolderLocationReader = value;
  }

  protected override SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return MockSettingsFolderLocationReader;
  }
}