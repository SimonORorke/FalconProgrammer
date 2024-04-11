using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FalconProgrammer.Tests.Model;

public static class XmlTestHelper {
  [ExcludeFromCodeCoverage]
  public static string GetEmbeddedResourceName(
    string embeddedResourceFileName, Assembly assembly) {
    if (string.IsNullOrWhiteSpace(embeddedResourceFileName)) {
      throw new InvalidOperationException(
        "An embedded resource file name has not been specified.");
    }
    string[] resourceNames = assembly.GetManifestResourceNames();
    string result;
    try {
      result = resourceNames.Single(
        resourcePath => resourcePath.Contains($".{embeddedResourceFileName}"));
      // For unknown reason, EndsWith does not work here. It used to.
      //   .First(resourcePath => resourcePath.EndsWith($".{embeddedResourceFileName}"));
    } catch (InvalidOperationException exception) {
      // Exception message is 'Sequence contains no matching element'
      throw new InvalidOperationException(
        $"'{embeddedResourceFileName}' is not in assembly " +
        $"{assembly.GetName().Name}, or it is not an EmbeddedResource file.",
        exception);
    }
    return result;
  }
}