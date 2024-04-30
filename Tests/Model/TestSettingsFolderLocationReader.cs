using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   For model tests.  Use <see cref="MockSettingsFolderLocationReader" /> for view
///   model tests.
/// </summary>
public class TestSettingsFolderLocationReader : SettingsFolderLocationReader {
  public TestSettingsFolderLocationReader() {
    AppDataFolderName = SettingsTestHelper.TestAppDataFolderName;
    FileSystemService = MockFileSystemService = new MockFileSystemService();
    Deserialiser = TestDeserialiser = new TestDeserialiser<SettingsFolderLocation>();
  }

  /// <summary>
  ///   The embedded resource file with this name in the Tests assembly will be
  ///   deserialised by the <see cref="SettingsFolderLocationReader.Read" /> method.
  /// </summary>
  internal string EmbeddedFileName {
    [ExcludeFromCodeCoverage] [PublicAPI] get => TestDeserialiser.EmbeddedFileName;
    set => TestDeserialiser.EmbeddedFileName = value;
  }

  [ExcludeFromCodeCoverage]
  [PublicAPI]
  internal MockFileSystemService MockFileSystemService { get; }

  protected TestDeserialiser<SettingsFolderLocation> TestDeserialiser { get; }
}