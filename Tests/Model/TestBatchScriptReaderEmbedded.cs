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
  }

  /// <summary>
  ///   The embedded resource file with this name in the Tests assembly will be
  ///   deserialised by the <see cref="Read" /> method, which will ignore its
  ///   batchScriptPath parameter.
  /// </summary>
  internal string EmbeddedFileName {
    [ExcludeFromCodeCoverage] [PublicAPI] get => TestDeserialiser.EmbeddedFileName;
    set => TestDeserialiser.EmbeddedFileName = value;
  }

  internal MockFileSystemService MockFileSystemService { get; }

  internal MockSerialiser MockSerialiserForBatchScript { get; } = new MockSerialiser();

  private TestDeserialiser<BatchScript> TestDeserialiser { get; }

  /// <summary>
  ///   Reads the embedded resource file specified by <see cref="EmbeddedFileName" /> in
  ///   the Tests assembly, ignoring the <paramref name="batchScriptPath" /> parameter.
  /// </summary>
  public override BatchScript Read(string batchScriptPath) {
    var result = base.Read(batchScriptPath);
    result.AppDataFolderName = AppDataFolderName;
    result.FileSystemService = FileSystemService;
    result.Serialiser = MockSerialiserForBatchScript;
    return result;
  }
}