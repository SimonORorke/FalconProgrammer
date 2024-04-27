using System.Xml.Serialization;

namespace FalconProgrammer.Model;

public class BatchSettings {
  [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
  [XmlAttribute] public string Category { get; set; } = string.Empty;
  [XmlAttribute] public string Program { get; set; } = string.Empty;
}