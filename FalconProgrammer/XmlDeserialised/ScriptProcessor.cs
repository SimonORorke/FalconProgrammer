using System.Xml.Serialization;
using FalconProgrammer.XmlLinq;

namespace FalconProgrammer.XmlDeserialised; 

public class ScriptProcessor {
  [XmlAttribute] public string Name { get; set; } = null!;
  
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<SignalConnection> SignalConnections { get; set; } = null!;

  internal ProgramXml ProgramXml { get; set; } = null!;
  
  public List<SignalConnection> GetSignalConnectionsWithCcNo(int ccNo) {
    return (
      from signalConnection in SignalConnections
      where signalConnection.CcNo == ccNo
      select signalConnection).ToList();
  }

  /// <summary>
  ///   Removes <see cref="SignalConnection" />s with the specified
  ///   <see cref="SignalConnection.Destination"/> from the Linq For XML data
  ///   structure as well as from the <see cref="ScriptProcessor" /> in the deserialised
  ///   data structure.
  /// </summary>
  public void RemoveSignalConnectionsWithDestination(string destination) {
    for (int i = SignalConnections.Count - 1; i >= 0; i--) {
      if (SignalConnections[i].Destination == destination) {
        SignalConnections.Remove(SignalConnections[i]);
      }
    }
    ProgramXml.RemoveSignalConnectionElementsWithDestination(destination);
  }
}