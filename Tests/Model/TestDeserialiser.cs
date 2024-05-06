using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class TestDeserialiser<T> : Deserialiser<T>
  where T : SerialisationBase {
  /// <summary>
  ///   <see cref="Deserialise(string)" /> will deserialise the embedded resource file
  ///   with this name in the Tests assembly, ignoring its inputPath parameter.
  /// </summary>
  public string EmbeddedFileName { get; set; } = string.Empty;

  /// <summary>
  ///   The file specified by <paramref name="inputPath" /> will not be accessed or
  ///   deserialised. Instead, the embedded resource file specified by
  ///   <see cref="EmbeddedFileName" /> will be read from the Tests assembly and
  ///   deserialised.
  /// </summary>
  public override T Deserialise(string inputPath) {
    return Deserialise(Global.GetEmbeddedFileStream(EmbeddedFileName));
  }
}