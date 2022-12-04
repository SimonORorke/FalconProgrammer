using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq;

public class ScriptProgramXml : ProgramXml {
  public ScriptProgramXml(string templatePath, ProgramConfig programConfig) : base(
    templatePath, programConfig) { }

  protected override XElement GetTemplateSignalConnectionElement() {
    var rootElement = XElement.Load(TemplatePath);
    var scriptProcessorElement =
      rootElement.Descendants("ScriptProcessor").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ScriptProcessor element in '{TemplatePath}'.");
    var result =
      scriptProcessorElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ScriptProcessor.SignalConnection element in '{TemplatePath}'.");
    return result;
  }
}