using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class Folder {
  [XmlAttribute] public string Path { get; set; } = string.Empty;
}