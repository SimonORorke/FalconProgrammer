using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised; 

[XmlRoot("UVI4")] public class UviRoot {
  [XmlElement] public ProgramDeserialised Program { get; set; } = null!;
}