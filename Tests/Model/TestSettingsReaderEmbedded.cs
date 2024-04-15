﻿using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A test Settings reader that reads embedded files.
/// </summary>
public class TestSettingsReaderEmbedded : SettingsReader {
  private Deserialiser<Settings>? _deserialiser;
  private MockFileSystemService? _mockFileSystemService;
  private MockSerialiser? _mockSerialiserForSettings;

  internal string EmbeddedSettingsFolderLocationFileName { get; set; } =
    "SettingsFolderLocation.xml";

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
    return new MockSettingsFolderLocationReader {
      FileSystemService = MockFileSystemService,
      TestDeserialiser = {
        EmbeddedResourceFileName = EmbeddedSettingsFolderLocationFileName
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