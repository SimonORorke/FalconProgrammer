using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class SoundBankCategorySetting {
  [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
  [XmlAttribute] public string Category { get; set; } = string.Empty;
}