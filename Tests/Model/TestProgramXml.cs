﻿using System.Reflection;
using System.Xml.Linq;
using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

public class TestProgramXml(Category category) : ProgramXml(category) {
  /// <summary>
  ///   <see cref="ReadRootElementFromFile" /> will read the embedded resource file
  ///   with this name in the Tests assembly, ignoring its programPath parameter.
  /// </summary>
  public string EmbeddedProgramFileName { get; set; } = "NoGuiScriptProcessor.uvip";
  
  /// <summary>
  ///   The file specified by <paramref name="programPath" /> will not be accessed or
  ///   deserialised. Instead, the embedded resource file specified by
  ///   <see cref="EmbeddedProgramFileName" /> will be read from the Tests assembly and
  ///   deserialised.
  /// </summary>
  protected override XElement ReadRootElementFromFile(string programPath) {
    var assembly = Assembly.GetExecutingAssembly();
    string resourceName = XmlTestHelper.GetEmbeddedResourceName(
      EmbeddedProgramFileName, assembly);
    return ReadRootElementFromStream(assembly.GetManifestResourceStream(resourceName)!);
  }
}