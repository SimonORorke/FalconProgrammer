using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised;

public class ScriptProcessor : INamed {
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<SignalConnection> SignalConnections { get; set; } = null!;

  [XmlElement("script")] public string Script { get; set; } = null!;
  [XmlAttribute] public string Name { get; set; } = null!;
}