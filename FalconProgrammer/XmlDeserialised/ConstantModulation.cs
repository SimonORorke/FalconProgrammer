using System.Xml.Serialization;
using JetBrains.Annotations;

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

  public int Index { get; set; }

  [PublicAPI] public bool IsContinuous =>
    Style == 0 || (Style == 1
      ? false
      : throw new NotSupportedException($"Style {Style} is not supported."));
  
  [PublicAPI] public int MacroNo => Convert.ToInt32(
    Name.Replace("Macro ", string.Empty));

  public int GetCcNo(ref int nextContinuousCcNo, ref int nextSwitchCcNo) {
    int result;
    if (IsContinuous) {
      // Assign continuous controller CC number to continuous macro. 
      // Convert the fifth continuous controller's CC number to 11 to map to the touch
      // slider.
      result = nextContinuousCcNo != 35 ? nextContinuousCcNo : 11;
      nextContinuousCcNo++;
    } else {
      // Assign button CC number to switch macro. 
      result = nextSwitchCcNo;
      nextSwitchCcNo++;
    }
    return result;
  }

  public override string ToString() {
    return $"{DisplayName} ({Name})";
  }
}