using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A test Settings reader that reads embedded files. 
/// </summary>
public class TestSettingsReaderEmbedded : SettingsReader {
  private Deserialiser<Settings>? _deserialiser;
  private MockSerialiser? _mockSerialiserForSettings;
  private MockFileSystemService? _mockFileSystemService;

  internal MockFileSystemService MockFileSystemService {
    get => _mockFileSystemService ??= new MockFileSystemService();
    set {
      _mockFileSystemService = value;
      FileSystemService = value;
    }
  }

  protected override SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return new TestSettingsFolderLocationReader {
      FileSystemService = MockFileSystemService,
      TestDeserialiser = {
        EmbeddedResourceFileName = "SettingsFolderLocation.xml"
      }
    };
  }

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
    result.FileSystemService = FileSystemService;
    result.Serialiser = MockSerialiserForSettings;
    return result;
  }
}