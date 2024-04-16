using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A test Settings reader that reads embedded files.
///   For model tests.  Use <see cref="MockSettingsReaderEmbedded" /> for view
///   model tests.
/// </summary>
public class TestSettingsReaderEmbedded : SettingsReader {
  private Deserialiser<Settings>? _deserialiser;
  private MockFileSystemService? _mockFileSystemService;
  private MockSerialiser? _mockSerialiserForSettings;

  internal MockFileSystemService MockFileSystemService {
    get => _mockFileSystemService ??= new MockFileSystemService();
    set {
      _mockFileSystemService = value;
      FileSystemService = value;
    }
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

  protected override SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return new TestSettingsFolderLocationReader {
      FileSystemService = MockFileSystemService,
      TestDeserialiser = {
        EmbeddedFileName = "SettingsFolderLocation.xml"
      }
    };
  }

  public override Settings Read(bool useDefaultIfNotFound = false) {
    var result = base.Read(useDefaultIfNotFound);
    result.FileSystemService = FileSystemService;
    result.Serialiser = MockSerialiserForSettings;
    return result;
  }
}