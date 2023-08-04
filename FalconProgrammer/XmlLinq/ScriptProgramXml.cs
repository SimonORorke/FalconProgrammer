using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class ScriptProgramXml : ProgramXml {
  public ScriptProgramXml(Category category) : base(category) { }

  protected override XElement GetTemplateSignalConnectionElement() {
    var rootElement = XElement.Load(Category.TemplateProgramPath);
    var scriptProcessorElement =
      (from s in rootElement.Descendants("ScriptProcessor")
        select s).LastOrDefault() ??
      throw new ApplicationException(
        $"'{InputProgramPath}': Cannot find ScriptProcessor element " +
        $"in template file '{Category.TemplateProgramPath}'.");
    var result =
      scriptProcessorElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ScriptProcessor {scriptProcessorElement.Attribute("Name")!.Value} " +
        // $"Cannot find ScriptProcessor {Category.TemplateScriptProcessorName} " +
        $"SignalConnection element in template file '{Category.TemplateProgramPath}'.");
    return result;
  }
}