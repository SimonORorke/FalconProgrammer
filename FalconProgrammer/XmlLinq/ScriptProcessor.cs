using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq;

public class ScriptProcessor : ModulationsOwnerBase {
  private XElement? _scriptElement;
  
  public ScriptProcessor(XElement scriptProcessorElement, ProgramXml programXml) :
    base(programXml) {
    Element = scriptProcessorElement;
  }

  public string Script => ScriptElement.Value;
  
  private XElement ScriptElement => _scriptElement ??= GetScriptElement();

  private XElement GetScriptElement() {
    var result = Element.Element("script");
    if (result == null) {
      throw new InvalidOperationException(
        "Cannot find ScriptProcessor.script "
        + $"element in '{ProgramXml.Category.TemplateProgramPath}'.");
    }
    return result;
  }

  public void Remove() {
    var eventProcessorsElement = Element.Parent;
    // We need to remove the EventProcessors element, including all its
    // ScriptProcessor elements, if there are more than one.
    // Just removing the Info page CCs ScriptProcessor element will not work.
    // ReSharper disable once CommentTypo
    // Example: Factory\RetroWave 2.5\BAS Voltage Reso.
    eventProcessorsElement!.Remove();
  }

  public void UpdateModulationsFromTemplate(
    IEnumerable<Modulation> templateModulations) {
    foreach (var modulation in templateModulations) {
      AddModulation(modulation);
    }
  }
}