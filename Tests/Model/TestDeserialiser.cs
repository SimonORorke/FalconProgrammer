using System.Diagnostics;
using System.Reflection;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

internal class TestDeserialiser<T> : Deserialiser<T>
  where T : SerialisationBase {

  /// <summary>
  ///   If specified, <see cref="Deserialise(string)"/> will deserialise the embedded
  ///   resource file with this name in the Tests assembly, ignoring its inputPath
  ///   parameter.
  /// </summary>
  public string? EmbeddedResourceFileName { get; set; }

  /// <summary>
  ///   If <see cref="EmbeddedResourceFileName"/> is specified, the file specified by
  ///   <paramref name="inputPath"/> will not be accessed or deserialised. Instead,
  ///   the embedded resource file specified by <see cref="EmbeddedResourceFileName"/>
  ///   will be read from the Tests assembly and deserialised.
  /// </summary>
  public override T Deserialise(string inputPath) {
    if (EmbeddedResourceFileName == null) {
      return base.Deserialise(inputPath);
    }
    var assembly = Assembly.GetExecutingAssembly();
    string[] resourceNames = assembly.GetManifestResourceNames();
    string resourceName = resourceNames.Single(
      resourcePath => resourcePath.Contains($".{EmbeddedResourceFileName}"));
    // For unknown reason, EndsWith does not work here. It used to.
    //   .First(resourcePath => resourcePath.EndsWith($".{EmbeddedResourceFileName}"));
    return Deserialise(assembly.GetManifestResourceStream(resourceName)!);
  }
}