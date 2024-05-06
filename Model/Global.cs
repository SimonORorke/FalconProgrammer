﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FalconProgrammer.Model;

public static class Global {
  public static string ApplicationName { get; set; } = string.Empty;

  internal static Stream GetEmbeddedFileStream(string embeddedFileName) {
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
        resourcePath => resourcePath.Contains($".{embeddedResourceFileName}"));
      // For unknown reason, EndsWith does always not work here.
      // resourcePath => resourcePath.EndsWith($".{embeddedResourceFileName}"));
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