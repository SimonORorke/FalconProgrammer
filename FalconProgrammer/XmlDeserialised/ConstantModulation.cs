using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlDeserialised;

/// <summary>
///   Seems to be the same thing as a macro.
/// </summary>
public class ConstantModulation {
  [XmlAttribute] public string Name { get; set; } = null!;
  [XmlAttribute] public string DisplayName { get; set; } = null!;
  [XmlAttribute] public int Bipolar { get; set; }
  [XmlAttribute] public int Style { get; set; }
  [XmlAttribute] public float Value { get; set; }

  /// <summary>
  ///   For a ConstantModulation, there is 0 or 1 SignalConnection only, except where
  ///   there is a SignalConnection that maps to the modulation wheel (MIDI CC 1), in
  ///   which case there can be two SignalConnections. 
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

  [PublicAPI] public int MacroNo {
    get {
      // In most programs, ConstantModulation Names consist of "Macro " followed by the
      // macro number. In a few programs, such as 'Factory\Keys\Pure FM Tines', 
      // the Names start with "MacroKnob " instead. 
      string[] split = Name.Split();
      if (split.Length != 2 || !int.TryParse(split[1], out int macroNo)) {
        throw new NotSupportedException(
          $"'{Name}' is not a supported macro name.");
      }
      return macroNo;
    }
    set => Name = $"Macro {value}";
  }

  public override string ToString() {
    return $"{DisplayName} ({Name})";
  }
}