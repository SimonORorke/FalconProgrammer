using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class TestSettingsReader : SettingsReader {
  private Deserialiser<Settings>? _deserialiser;
  private MockFileSystemService? _mockFileSystemServiceForSettings;
  private MockSerialiser? _mockSerialiserForSettings;

  protected override SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return new TestSettingsFolderLocationReader {
      TestDeserialiser = {
        EmbeddedResourceFileName = "SettingsFolderLocation.xml"
      }
    };
  }

  // internal MockFileSystemService MockFileSystemServiceForSettings {
  //   get => _mockFileSystemServiceForSettings ??= new MockFileSystemService();
  //   set => _mockFileSystemServiceForSettings = value;
  // }

  internal MockSerialiser MockSerialiserForSettings {
    get => _mockSerialiserForSettings ??= new MockSerialiser();
    set => _mockSerialiserForSettings = value;
  }

  internal TestDeserialiser<Settings> TestDeserialiser =>
    (TestDeserialiser<Settings>)Deserialiser;  

  private new Deserialiser<Settings> Deserialiser {
    get {
      if (_deserialiser == null) {
        base.Deserialiser = _deserialiser = new TestDeserialiser<Settings>(); 
      }
      return _deserialiser;
    }
  }

  public override Settings Read(bool useDefaultIfNotFound = false) {
    var result = base.Read(useDefaultIfNotFound);
    // result.FileSystemService = MockFileSystemServiceForSettings;
    result.Serialiser = MockSerialiserForSettings;
    return result;
  }
}