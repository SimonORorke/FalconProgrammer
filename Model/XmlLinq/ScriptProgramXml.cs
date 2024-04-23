using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptProgramXml : ProgramXml {
  public ScriptProgramXml(Category category) : base(category) { }

  protected override XElement GetTemplateModulationElement() {
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
        $"Modulation element in template file '{Category.TemplateProgramPath}'.");
    return result;
  }
}