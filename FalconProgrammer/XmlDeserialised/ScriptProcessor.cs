using System.Xml.Linq;
using System.Xml.Serialization;
using FalconProgrammer.XmlLinq;

namespace FalconProgrammer.XmlDeserialised;

public class ScriptProcessor : INamed {
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<Modulation> Modulations { get; set; } = null!;

  [XmlElement("script")] public string Script { get; set; } = null!;
  internal ProgramXml ProgramXml { get; set; } = null!;
  internal XElement ScriptProcessorElement { get; set; } = null!;
  [XmlAttribute] public string Name { get; set; } = null!;

  /// <summary>
  ///   Adds the specified <see cref="Modulation" /> to the ScriptProcessor
  ///   in the Linq For XML data structure as well as in the deserialised data structure.
  /// </summary>
  public void AddModulation(Modulation modulation) {
    Modulations.Add(modulation);
    var connectionsElement = ScriptProcessorElement.Element("Connections") ??
                             new XElement("Connections");
    ScriptProcessorElement.Add(connectionsElement);
    connectionsElement.Add(ProgramXml.CreateModulationElement(modulation));
  }
}