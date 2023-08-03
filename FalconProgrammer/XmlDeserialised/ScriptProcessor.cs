using System.Xml.Serialization;
using FalconProgrammer.XmlLinq;

namespace FalconProgrammer.XmlDeserialised; 

public class ScriptProcessor : INamed {
  [XmlAttribute] public string Name { get; set; } = null!;
  
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<SignalConnection> SignalConnections { get; set; } = null!;
}