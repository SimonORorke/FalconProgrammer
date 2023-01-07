using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class ScriptProgramXml : ProgramXml {
  public ScriptProgramXml(
    Category category, ScriptProcessor infoPageCcsScriptProcessor) : base(
    category, infoPageCcsScriptProcessor) { }

  protected override XElement GetTemplateSignalConnectionElement() {
    var rootElement = XElement.Load(Category.TemplateProgramPath);
    var scriptProcessorElement =
      (from s in rootElement.Descendants("ScriptProcessor")
        select s).LastOrDefault() ??
      throw new ApplicationException(
        "Cannot find ScriptProcessor element " +
        $"in template file '{Category.TemplateProgramPath}'.");
    // var scriptProcessorElements = 
    //   rootElement.Descendants("ScriptProcessor").ToList();
    // if (!scriptProcessorElements.Any()) {
    //   throw new ApplicationException(
    //     "Cannot find any ScriptProcessor elements " +
    //     $"in template file '{Category.TemplateProgramPath}'.");
    // }
    // var scriptProcessorElement =
    //   (from s in scriptProcessorElements
    //     where s.Attribute("Name")!.Value == Category.TemplateScriptProcessorName
    //     select s).FirstOrDefault() ??
    //   throw new ApplicationException(
    //     "Cannot find ScriptProcessor element " +
    //     $"{Category.TemplateScriptProcessorName} in template file '{Category.TemplateProgramPath}'.");
    var result =
      scriptProcessorElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ScriptProcessor {scriptProcessorElement.Attribute("Name")!.Value} " +
        // $"Cannot find ScriptProcessor {Category.TemplateScriptProcessorName} " +
        $"SignalConnection element in template file '{Category.TemplateProgramPath}'.");
    return result;
  }
}