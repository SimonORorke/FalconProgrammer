using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A test Settings reader that reads embedded files.
///   For model tests.  Use <see cref="MockSettingsReaderEmbedded" /> for view
///   model tests.
/// </summary>
public class TestSettingsReaderEmbedded : SettingsReader {
  public TestSettingsReaderEmbedded() {
    AppDataFolderName = SettingsTestHelper.TestAppDataFolderName;
    FileSystemService = MockFileSystemService = new MockFileSystemService();
    Deserialiser = TestDeserialiser = new TestDeserialiser<Settings>();
    Serialiser = MockSerialiserForSettings = new MockSerialiser();
  }

  /// <summary>
  ///   The embedded resource file with this name in the Tests assembly will be
  ///   deserialised by the <see cref="Read" /> method.
  /// </summary>
  internal string EmbeddedFileName {
    [ExcludeFromCodeCoverage] [PublicAPI] get => TestDeserialiser.EmbeddedFileName;
    set => TestDeserialiser.EmbeddedFileName = value;
  }

  [PublicAPI] internal MockFileSystemService MockFileSystemService { get; }
  internal MockSerialiser MockSerialiserForSettings { get; }
  private TestDeserialiser<Settings> TestDeserialiser { get; }

  protected override SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return new TestSettingsFolderLocationReader {
      FileSystemService = MockFileSystemService,
      EmbeddedFileName = "SettingsFolderLocation.xml"
    };
  }

  public override Settings Read(bool useDefaultIfNotFound = false) {
    var result = base.Read(useDefaultIfNotFound);
    result.AppDataFolderName = AppDataFolderName;
    result.FileSystemService = FileSystemService;
    result.Serialiser = MockSerialiserForSettings;
    return result;
  }
}