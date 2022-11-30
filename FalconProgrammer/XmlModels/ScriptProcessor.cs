using System.Xml.Serialization;

namespace FalconProgrammer.XmlModels; 

public class ScriptProcessor {
  [XmlAttribute] public string Name { get; set; } = null!;
  
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<SignalConnection> SignalConnections { get; set; } = null!;
}