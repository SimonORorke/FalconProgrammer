using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A test <see cref="BatchScript" /> reader that reads embedded files.
/// </summary>
public class TestBatchScriptReaderEmbedded : BatchScriptReader {
  public TestBatchScriptReaderEmbedded() {
    AppDataFolderName = SettingsTestHelper.TestAppDataFolderName;
    FileSystemService = MockFileSystemService = new MockFileSystemService();
    Deserialiser = TestDeserialiser = new TestDeserialiser<BatchScript>();
    Serialiser = MockSerialiserForBatchScript = new MockSerialiser();
  }

  /// <summary>
  ///   The embedded resource file with this name in the Tests assembly will be
  ///   deserialised by the <see cref="BatchScriptReader.Read" /> method, which will
  ///   ignore its batchScriptPath parameter.
  /// </summary>
  internal string EmbeddedFileName {
    [ExcludeFromCodeCoverage] [PublicAPI] get => TestDeserialiser.EmbeddedFileName;
    set => TestDeserialiser.EmbeddedFileName = value;
  }

  internal MockFileSystemService MockFileSystemService { get; }
  internal MockSerialiser MockSerialiserForBatchScript { get; }
  private TestDeserialiser<BatchScript> TestDeserialiser { get; }
}