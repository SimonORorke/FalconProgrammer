using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockSettingsReaderEmbedded : TestSettingsReaderEmbedded {
  protected override SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return new MockSettingsFolderLocationReader {
      FileSystemService = MockFileSystemService,
      TestDeserialiser = {
        EmbeddedResourceFileName = EmbeddedSettingsFolderLocationFileName
      }
    };
  }
}