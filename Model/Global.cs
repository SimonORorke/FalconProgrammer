using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FalconProgrammer.Model;

public static class Global {
  private static string? _applicationName;

  public static string ApplicationFolderPath => AppDomain.CurrentDomain.BaseDirectory;

  public static string ApplicationName {
    get => _applicationName ??= new ApplicationInfo().Product;
    set => _applicationName = value; // For tests
  }

  public static bool EmbeddedFileExists(string embeddedFileName) {
    var assembly = Assembly.GetCallingAssembly();
    try {
      GetEmbeddedResourceName(embeddedFileName, assembly);
      return true;
    } catch (InvalidOperationException) {
      return false;
    }
  }

  public static Stream GetEmbeddedFileStream(string embeddedFileName) {
    var assembly = Assembly.GetCallingAssembly();
    string resourceName = GetEmbeddedResourceName(
      embeddedFileName, assembly);
    return assembly.GetManifestResourceStream(resourceName)!;
  }

  [ExcludeFromCodeCoverage]
  private static string GetEmbeddedResourceName(
    string embeddedResourceFileName, Assembly assembly) {
    if (string.IsNullOrWhiteSpace(embeddedResourceFileName)) {
      throw new InvalidOperationException(
        "An embedded resource file name has not been specified.");
    }
    string[] resourceNames = assembly.GetManifestResourceNames();
    string result;
    try {
      result = resourceNames.Single(
        resourcePath => resourcePath.EndsWith($".{embeddedResourceFileName}"));
    } catch (InvalidOperationException exception) {
      // If not found, the exception message is 'Sequence contains no matching element'.
      throw new InvalidOperationException(
        $"'{embeddedResourceFileName}' is not in assembly " +
        $"{assembly.GetName().Name}, or it is not an EmbeddedResource file, " +
        "or there is another EmbeddedResource file with the same file name in the " +
        "assembly.",
        exception);
    }
    return result;
  }

  public static T GetEnumValue<T>(string name) where T : Enum {
    var names = Enum.GetNames(typeof(T)).ToList();
    var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
    if (names.Contains(name)) {
      return (
        from enumValue in values
        where enumValue.ToString() == name
        select enumValue).Single();
    }
    return values[0];
  }
}