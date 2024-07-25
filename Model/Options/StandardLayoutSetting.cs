using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class StandardLayoutSetting {
  [XmlAttribute] public bool StandardLayout { get; set; }
}