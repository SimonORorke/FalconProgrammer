using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

internal class TestDeserialiser<T> : Deserialiser<T>
  where T : SerialisationBase {

  /// <summary>
  ///   <see cref="Deserialise(string)"/> will deserialise the embedded resource file
  ///   with this name in the Tests assembly, ignoring its inputPath parameter.
  /// </summary>
  public string EmbeddedResourceFileName { get; set; } = string.Empty;

  /// <summary>
  ///   The file specified by <paramref name="inputPath"/> will not be accessed or
  ///   deserialised. Instead, the embedded resource file specified by
  ///   <see cref="EmbeddedResourceFileName"/> will be read from the Tests assembly and
  ///   deserialised.
  /// </summary>
  public override T Deserialise(string inputPath) {
    var assembly = Assembly.GetExecutingAssembly();
    string resourceName = GetResourceName(assembly);
    return Deserialise(assembly.GetManifestResourceStream(resourceName)!);
  }

  [ExcludeFromCodeCoverage]
  private string GetResourceName(Assembly assembly) {
    if (string.IsNullOrWhiteSpace(EmbeddedResourceFileName)) {
      throw new InvalidOperationException(
        "EmbeddedResourceFileName has not been specified.");
    }
    string[] resourceNames = assembly.GetManifestResourceNames();
    string result;
    try {
      result = resourceNames.Single(
        resourcePath => resourcePath.Contains($".{EmbeddedResourceFileName}"));
      // For unknown reason, EndsWith does not work here. It used to.
      //   .First(resourcePath => resourcePath.EndsWith($".{EmbeddedResourceFileName}"));
    } catch (InvalidOperationException exception) {
      // Exception message is 'Sequence contains no matching element'
      throw new InvalidOperationException(
        $"'{EmbeddedResourceFileName}' is not in assembly " + 
        $"{assembly.GetName().Name}, or it is not an EmbeddedResource file.", 
        exception);
    }
    return result;
  }
}