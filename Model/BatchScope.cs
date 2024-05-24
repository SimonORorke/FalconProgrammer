using System.Xml.Serialization;

namespace FalconProgrammer.Model;

public class BatchScope {
  [XmlElement] public string SoundBank { get; set; } = string.Empty;
  [XmlElement] public string Category { get; set; } = string.Empty;
  [XmlElement] public string Program { get; set; } = string.Empty;
}