using System.Xml;
using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised;

public class ConstantModulation {
  [XmlAttribute] public string Name { get; set; } = null!;
  [XmlAttribute] public string DisplayName { get; set; } = null!;
  [XmlAttribute] public int Style { get; set; }

  /// <summary>
  ///   For a ConstantModulation, there is 0 or 1 SignalConnection only. 
  /// </summary>
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<SignalConnection> SignalConnections { get; set; } = null!;

  [XmlElement] public Properties Properties { get; set; } = null!;

  public bool IsContinuous =>
    Style == 0 || (Style == 1
      ? false
      : throw new XmlException($"Invalid Style {Style}"));
  
  public int MacroNo => Convert.ToInt32(
    Name.Replace("Macro ", string.Empty));
}