using System.Collections.Immutable;
using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class ConnectionsParent : INamed {
  private ImmutableList<Modulation>? _modulations;

  public ConnectionsParent(XElement connectionsParentElement, ProgramXml programXml) {
    ConnectionsParentElement = connectionsParentElement;
    ProgramXml = programXml;
    Name = ConnectionsParentElement.Name.ToString();
    // Name = ProgramXml.GetAttributeValue(ConnectionsParentElement, nameof(Name));
  }

  public bool Bypass {
    get => ProgramXml.GetAttributeValue(ConnectionsParentElement, "Bypass") == "1";
    set => ProgramXml.SetAttribute(ConnectionsParentElement, "Bypass", value ? "1" : "0");
  }

  private XElement ConnectionsParentElement { get; }
  private ProgramXml ProgramXml { get; }

  /// <summary>
  ///   Modulations specifying modulators of the effect.
  /// </summary>
  public ImmutableList<Modulation> Modulations {
    get => _modulations ??= GetModulations();
    private set => _modulations = value;
  }

  public string Name { get; }

  public void ChangeModulatedParametersToZero() {
    foreach (var modulation in Modulations) {
      try {
        ProgramXml.SetAttribute(
          ConnectionsParentElement, modulation.Destination,
          // If it's a toggle macro, Destination should be "Bypass".  
          modulation.Destination == "Bypass" ? 1 : 0);
      } catch (InvalidOperationException) {
        // Cannot find attribute. So the modulation is trying to modulate an
        // effect parameter that does not exist.
        // Example: Ether Fields\Bells - Plucks\Bali Plucker
      }
    }
  }

  private ImmutableList<Modulation> GetModulations() {
    var list = new List<Modulation>();
    var connectionsElement = ConnectionsParentElement.Element("Connections");
    if (connectionsElement != null) {
      list.AddRange(connectionsElement.Elements("SignalConnection").Select(
        modulationElement => new Modulation(
          this, modulationElement)));
    }
    return list.ToImmutableList();
  }

  public void RemoveModulationsByMacro(Macro macro) {
    var modulations = Modulations.ToList();
    for (int i = modulations.Count - 1; i >= 0; i--) {
      if (modulations[i].SourceMacro == macro) {
        modulations[i].ModulationElement!.Remove();
        modulations.Remove(modulations[i]);
      }
    }
    Modulations = modulations.ToImmutableList();
    var connectionsElement = ConnectionsParentElement.Element("Connections");
    if (connectionsElement != null && !connectionsElement.Nodes().Any()) {
      connectionsElement.Remove();
    }
  }
}