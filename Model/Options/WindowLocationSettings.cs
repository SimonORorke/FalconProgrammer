using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class WindowLocationSettings {
  [XmlAttribute] public int Left { get; set; }
  [XmlAttribute] public int Top { get; set; }
  [XmlAttribute] public int Width { get; set; }
  [XmlAttribute] public int Height { get; set; }
  [XmlAttribute] public int WindowState { get; set; }
}