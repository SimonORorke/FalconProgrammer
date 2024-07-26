using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class BackgroundSetting {
  [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
  [XmlAttribute] public string Path { get; set; } = string.Empty;
}