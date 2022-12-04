using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised; 

public class OrganicKeysScriptProcessor : ScriptProcessor {
  [XmlAttribute("delaySend")] public float DelaySend { get; set; }
  [XmlAttribute("reverbSend")] public float ReverbSend { get; set; }
}