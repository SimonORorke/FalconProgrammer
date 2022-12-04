using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised;

public class Properties {
  [XmlAttribute("x")] public int X { get; set; }
  [XmlAttribute("y")] public int Y { get; set; }
}