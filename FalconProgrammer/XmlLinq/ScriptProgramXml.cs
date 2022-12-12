using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class ScriptProgramXml : ProgramXml {
  public ScriptProgramXml(
    string templateProgramPath, ScriptProcessor infoPageCcsScriptProcessor) : base(
    templateProgramPath, infoPageCcsScriptProcessor) { }

  protected override XElement GetTemplateSignalConnectionElement() {
    var rootElement = XElement.Load(TemplateProgramPath);
    var scriptProcessorElements = 
      rootElement.Descendants("ScriptProcessor").ToList();
    if (!scriptProcessorElements.Any()) {
      throw new ApplicationException(
        "Cannot find any ScriptProcessor elements " +
        $"in template file '{TemplateProgramPath}'.");
    }
    var scriptProcessorElement =
      (from s in scriptProcessorElements
        where s.Attribute("Name")!.Value == InfoPageCcsScriptProcessor!.Name
        select s).FirstOrDefault() ??
      throw new ApplicationException(
        "Cannot find ScriptProcessor element " +
        $"{InfoPageCcsScriptProcessor!.Name} in template file '{TemplateProgramPath}'.");
    var result =
      scriptProcessorElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ScriptProcessor {InfoPageCcsScriptProcessor!.Name} " +
        $"SignalConnection element in template file '{TemplateProgramPath}'.");
    return result;
  }
}