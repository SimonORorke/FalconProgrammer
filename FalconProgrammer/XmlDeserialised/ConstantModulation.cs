using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlDeserialised;

public class ConstantModulation {
  [XmlAttribute] public string Name { get; set; } = null!;
  [XmlAttribute] public string DisplayName { get; set; } = null!;
  [XmlAttribute] public int Bipolar { get; set; } // bool ?
  [XmlAttribute] public int Style { get; set; }
  [XmlAttribute] public float Value { get; set; }

  /// <summary>
  ///   For a ConstantModulation, there is 0 or 1 SignalConnection only. 
  /// </summary>
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<SignalConnection> SignalConnections { get; set; } = null!;

  [XmlElement] public Properties Properties { get; set; } = null!;

  public int Index { get; set; }

  public bool IsContinuous {
    get =>
      Style == 0 || (Style == 1
        ? false
        : throw new NotSupportedException($"Style {Style} is not supported."));
    set => Style = value ? 0 : 1;
  }

  public bool IsMacro => MacroNo != null;

  [PublicAPI] public int? MacroNo {
    get {
      if (!Name.StartsWith("Macro ")) {
        return null;
      }
      if (!int.TryParse(
            Name.Replace("Macro ", string.Empty), out int macroNo)) {
        return null;
      }
      return macroNo;
    }
    set => Name = $"Macro {value}";
  }

  public override string ToString() {
    return $"{DisplayName} ({Name})";
  }
}