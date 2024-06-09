using System.Collections.Immutable;
using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

internal class ConnectionsParent : EntityBase {
  private ImmutableList<Modulation>? _modulations;

  public ConnectionsParent(XElement element, ProgramXml programXml, MidiForMacros midi)
    : base(programXml) {
    Element = element;
    Midi = midi;
  }

  private MidiForMacros Midi { get; }

  /// <summary>
  ///   Modulations specifying MIDI CC numbers that modulate the effect.
  /// </summary>
  public ImmutableList<Modulation> Modulations {
    get => _modulations ??= GetModulations();
    private set => _modulations = value;
  }

  public override string Name => Element.Name.ToString();

  public void ChangeModulatedParametersToZero() {
    foreach (var modulation in Modulations) {
      try {
        SetAttribute(modulation.Destination,
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
    var connectionsElement = Element.Element("Connections");
    if (connectionsElement != null) {
      list.AddRange(connectionsElement.Elements("SignalConnection").Select(
        modulationElement => new Modulation(
          this, modulationElement, ProgramXml, Midi)));
    }
    return list.ToImmutableList();
  }

  public void RemoveModulationsByMacro(Macro macro) {
    var modulations = Modulations.ToList();
    for (int i = modulations.Count - 1; i >= 0; i--) {
      if (modulations[i].SourceMacro == macro) {
        modulations[i].Element.Remove();
        modulations.Remove(modulations[i]);
      }
    }
    Modulations = modulations.ToImmutableList();
    var connectionsElement = Element.Element("Connections");
    if (connectionsElement != null && !connectionsElement.Nodes().Any()) {
      connectionsElement.Remove();
    }
  }
}