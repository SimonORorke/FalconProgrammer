using System.Collections.Immutable;
using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq;

public class ScriptProcessor : EntityBase {
  public ScriptProcessor(XElement scriptProcessorElement, ProgramXml programXml) :
    base(programXml) {
    Element = scriptProcessorElement;
  }

  private XElement? ConnectionsElement { get; set; }

  public ImmutableList<Modulation> Modulations { get; private set; } =
    ImmutableList<Modulation>.Empty;
  
  public string Script {
    get => GetAttributeValue(nameof(Script).ToLower());
    set => SetAttribute(nameof(Script).ToLower(), value);
  }

  public string Source => GetAttributeValue(nameof(Source).ToLower());

  public void AddModulation(Modulation modulation) {
    Modulations = Modulations.Add(modulation); 
    if (ConnectionsElement == null) {
      var existingConnectionsElement = Element.Element("Connections");
      ConnectionsElement = existingConnectionsElement ?? new XElement("Connections");
      Element.Add(ConnectionsElement);
    }
    ConnectionsElement.Add(modulation.Element);
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

  public void RemoveModulation(Modulation modulation) {
    Modulations = Modulations.Remove(modulation); 
    modulation.Element.Remove();
    if (ConnectionsElement is { HasElements: false }) {
      ConnectionsElement.Remove();
      ConnectionsElement = null;
    }
  }

  public void UpdateModulationsFromTemplate(
    IEnumerable<Modulation> templateModulations) {
    Modulations = Modulations.AddRange(templateModulations);
  }
}