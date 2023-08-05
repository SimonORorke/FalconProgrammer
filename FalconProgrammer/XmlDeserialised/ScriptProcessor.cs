﻿using System.Xml.Linq;
using System.Xml.Serialization;
using FalconProgrammer.XmlLinq;

namespace FalconProgrammer.XmlDeserialised;

public class ScriptProcessor : INamed {
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<SignalConnection> SignalConnections { get; set; } = null!;

  [XmlElement("script")] public string Script { get; set; } = null!;
  internal ProgramXml ProgramXml { get; set; } = null!;
  internal XElement ScriptProcessorElement { get; set; } = null!;
  [XmlAttribute] public string Name { get; set; } = null!;

  /// <summary>
  ///   Adds the specified <see cref="SignalConnection" /> to the ScriptProcessor
  ///   in the Linq For XML data structure as well as in the deserialised data structure.
  /// </summary>
  public void AddSignalConnection(SignalConnection signalConnection) {
    SignalConnections.Add(signalConnection);
    var connectionsElement = ScriptProcessorElement.Element("Connections") ??
                             new XElement("Connections");
    ScriptProcessorElement.Add(connectionsElement);
    connectionsElement.Add(ProgramXml.CreateSignalConnectionElement(signalConnection));
  }
}