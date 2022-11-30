using System.Xml.Serialization;

namespace FalconProgrammer.XmlModels; 

[XmlRoot("UVI4")] public class UviRoot {
  [XmlElement] public FalconProgram Program { get; set; } = null!;
}