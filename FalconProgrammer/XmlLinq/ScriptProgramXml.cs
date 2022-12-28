﻿using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class ScriptProgramXml : ProgramXml {
  public ScriptProgramXml(
    Category category, ScriptProcessor infoPageCcsScriptProcessor) : base(
    category, infoPageCcsScriptProcessor) { }

  protected override XElement GetTemplateSignalConnectionElement() {
    var rootElement = XElement.Load(Category.TemplateProgramPath);
    var scriptProcessorElements = 
      rootElement.Descendants("ScriptProcessor").ToList();
    if (!scriptProcessorElements.Any()) {
      throw new ApplicationException(
        "Cannot find any ScriptProcessor elements " +
        $"in template file '{Category.TemplateProgramPath}'.");
    }
    var scriptProcessorElement =
      (from s in scriptProcessorElements
        where s.Attribute("Name")!.Value == Category.InfoPageCcsScriptProcessorName
        select s).FirstOrDefault() ??
      throw new ApplicationException(
        "Cannot find ScriptProcessor element " +
        $"{InfoPageCcsScriptProcessor!.Name} in template file '{Category.TemplateProgramPath}'.");
    var result =
      scriptProcessorElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ScriptProcessor {Category.InfoPageCcsScriptProcessorName} " +
        $"SignalConnection element in template file '{Category.TemplateProgramPath}'.");
    return result;
  }
}