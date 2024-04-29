using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A test <see cref="BatchScript" /> reader that reads embedded files.
/// </summary>
public class TestBatchScriptReaderEmbedded : BatchScriptReader {
  /// <summary>
  ///   The embedded resource file with this name in the Tests assembly will be
  ///   deserialised by the <see cref="Read" /> method, which will ignore its
  ///   batchScriptPath parameter.
  /// </summary>
  internal string EmbeddedFileName {
    [ExcludeFromCodeCoverage] [PublicAPI] get => TestDeserialiser.EmbeddedFileName;
    set => TestDeserialiser.EmbeddedFileName = value;
  }

  public override IFileSystemService FileSystemService {
    get {
      if (!base.FileSystemService.Equals(MockFileSystemService)) {
        base.FileSystemService = MockFileSystemService;
      }
      return (MockFileSystemService)base.FileSystemService;
    }
    [ExcludeFromCodeCoverage] set => throw new NotSupportedException();
  }

  internal MockFileSystemService MockFileSystemService { get; } =
    new MockFileSystemService();

  internal MockSerialiser MockSerialiserForBatchScript { get; } = new MockSerialiser();

  private TestDeserialiser<BatchScript> TestDeserialiser { get; } =
    new TestDeserialiser<BatchScript>();

  internal override Deserialiser<BatchScript> Deserialiser {
    get {
      if (!base.Deserialiser.Equals(TestDeserialiser)) {
        base.Deserialiser = TestDeserialiser;
      }
      return (TestDeserialiser<BatchScript>)base.Deserialiser;
    }
    [ExcludeFromCodeCoverage] set => throw new NotSupportedException();
  }

  /// <summary>
  ///   Reads the embedded resource file specified by <see cref="EmbeddedFileName" /> in
  ///   the Tests assembly, ignoring the <paramref name="batchScriptPath" /> parameter.
  /// </summary>
  public override BatchScript Read(string batchScriptPath) {
    var result = base.Read(batchScriptPath);
    result.FileSystemService = FileSystemService;
    result.Serialiser = MockSerialiserForBatchScript;
    return result;
  }
}