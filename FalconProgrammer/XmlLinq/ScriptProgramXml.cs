using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class ScriptProgramXml : ProgramXml {
  public ScriptProgramXml(
    string templateProgramPath, ScriptProcessor infoPageCcsScriptProcessor) : base(
    templateProgramPath, infoPageCcsScriptProcessor) { }

  protected override XElement GetTemplateSignalConnectionElement() {
    var rootElement = XElement.Load(TemplateProgramPath);
    var scriptProcessorElement =
      rootElement.Descendants("ScriptProcessor").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ScriptProcessor element in '{TemplateProgramPath}'.");
    var result =
      scriptProcessorElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ScriptProcessor.SignalConnection element in '{TemplateProgramPath}'.");
    return result;
  }
}