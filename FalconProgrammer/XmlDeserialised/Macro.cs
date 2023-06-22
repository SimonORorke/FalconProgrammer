using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlDeserialised;

/// <summary>
///   Called ConstantModulation in the program XML but can only represent a macro, so far
///   as I can tell.
/// </summary>
public class Macro {
  /// <summary>
  ///   The macro name, which only indicates the macro number. For reference in
  ///   <see cref="SignalConnection" />s owned by effects that have parameters modulated
  ///   by the macro. Usually like 'Macro 3' but, in a few programs such as
  ///   'Factory\Keys\Pure FM Tines', like 'MacroKnob 3'.
  /// </summary>
  [XmlAttribute]
  public string Name { get; set; } = null!;

  /// <summary>
  ///   The meaningful name of the macro, as displayed on the Info page.
  /// </summary>
  [XmlAttribute]
  public string DisplayName { get; set; } = null!;

  [XmlAttribute] public int Bipolar { get; set; }

  /// <summary>
  ///   0 indicates a toggle macro. 1 indicates a continuous macro.
  /// </summary>
  [XmlAttribute]
  public int Style { get; set; }

  [XmlAttribute] public float Value { get; set; }

  /// <summary>
  ///   For a macro (ConstantModulation), there is 0 or 1 SignalConnection only, except
  ///   where there is a SignalConnection that maps to the modulation wheel (MIDI CC 1),
  ///   in which case there can be two SignalConnections.
  /// </summary>
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<SignalConnection> SignalConnections { get; set; } = null!;

  [XmlElement] public Properties Properties { get; set; } = null!;
  public bool ControlsDelay => DisplayName.Contains("Delay");

  public bool ControlsReverb => 
    DisplayName.Contains("Reverb")
    // Room reduced many Spectre programs silent when zeroed.
    // So leave it out till I get to the bottom of it.
    // || DisplayName.Contains("Room")
    || DisplayName.Contains("SparkVerb");

  /// <summary>
  ///   Indicates whether this is continuous macro.  If false, it's a toggle macro.
  /// </summary>
  public bool IsContinuous {
    get =>
      Style == 0 || (Style == 1
        ? false
        : throw new NotSupportedException($"Style {Style} is not supported."));
    set => Style = value ? 0 : 1;
  }

  [PublicAPI]
  public int MacroNo {
    get {
      // In most programs, macro (ConstantModulation) Names consist of "Macro " followed
      // by the macro number. In a few programs, such as 'Factory\Keys\Pure FM Tines', 
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