using System.Xml.Serialization;
using FalconProgrammer.XmlLinq;

namespace FalconProgrammer.XmlDeserialised; 

public class ScriptProcessor {
  [XmlAttribute] public string Name { get; set; } = null!;
  
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<SignalConnection> SignalConnections { get; set; } = null!;

  public List<SignalConnection> GetSignalConnectionsWithCcNo(int ccNo) {
    return (
      from signalConnection in SignalConnections
      where signalConnection.CcNo == ccNo
      select signalConnection).ToList();
  }
}