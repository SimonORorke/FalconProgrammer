﻿using System.Xml.Linq;
using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

public class TestProgramXml : ProgramXml {
  public TestProgramXml(Category category) : base(category) { }

  /// <summary>
  ///   <see cref="ReadProgramRootElementFromFile" /> will read the embedded resource
  ///   file with this name in the Tests assembly.
  /// </summary>
  internal string EmbeddedProgramFileName { get; set; } = "NoGuiScriptProcessor.xml";

  /// <summary>
  ///   <see cref="ReadTemplateRootElementFromFile" /> will read the embedded resource
  ///   file with this name in the Tests assembly.
  /// </summary>
  internal string EmbeddedTemplateFileName { get; set; } = "NoGuiScriptProcessor.xml";

  private void OnSaved(string savedXml) {
    Saved?.Invoke(this, savedXml);
  }

  /// <summary>
  ///   The embedded resource file specified by <see cref="EmbeddedProgramFileName" />
  ///   will be read from the Tests assembly and deserialised.
  /// </summary>
  protected override XElement ReadProgramRootElementFromFile() {
    var reader = new StreamReader(Global.GetEmbeddedFileStream(EmbeddedProgramFileName));
    string programXmlText = reader.ReadToEnd();
    return ReadRootElementFromXmlText(programXmlText);
  }

  /// <summary>
  ///   The embedded resource file specified by <see cref="EmbeddedTemplateFileName" />
  ///   will be read from the Tests assembly and deserialised.
  /// </summary>
  protected override XElement ReadTemplateRootElementFromFile() {
    var reader = new StreamReader(Global.GetEmbeddedFileStream(EmbeddedTemplateFileName));
    string programXmlText = reader.ReadToEnd();
    return ReadRootElementFromXmlText(programXmlText);
  }

  protected override void SaveXmlTextToFile(string outputProgramPath, string xmlText) {
    OnSaved(xmlText);
  }

  internal event EventHandler<string>? Saved;
}