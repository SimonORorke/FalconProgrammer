using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlDeserialised;

/// <summary>
///   Called ConstantModulation in the program XML but can only represent a macro, so far
///   as I can tell.
/// </summary>
public class Macro {
  /// <summary>
  ///   The macro name, which uniquely identifies the macro. For reference in
  ///   <see cref="SignalConnection" />s owned by effects or the
  ///   <see cref="FalconProgram.InfoPageCcsScriptProcessor" />.
  ///   only indicates the macro number. 
  /// </summary>
  /// <remarks>
  ///   The name format is usually like 'Macro 3' but, in a few programs such as
  ///   'Factory\Keys\Pure FM Tines', like 'MacroKnob 3'.  So a macro number
  ///   <see cref="MacroNo" /> is derived from the name.
  /// </remarks>
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
    || DisplayName.Contains("Room")
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

  /// <summary>
  ///   The macro number, derived from <see cref="Name" />.
  /// </summary>
  /// <remarks>
  ///   Though <see cref="Name" /> is a unique identifier, this derived macro number
  ///   cannot be 100% relied on to also be one. This is because there is at least one
  ///   program, Titanium\Pads\Children's Choir, that has macros named both "Macro 1"
  ///   and "MacroKnob 1" etc., presumably due to a programmers' oversight.
  /// </remarks>
  [PublicAPI]
  public int MacroNo {
    get {
      string[] split = Name.Split();
      if (split.Length != 2 || !int.TryParse(split[1], out int macroNo)) {
        throw new NotSupportedException(
          $"'{Name}' is not a supported macro name.");
      }
      return macroNo;
    }
    set => Name = $"Macro {value}";
  }

  public SignalConnection? FindSignalConnection(int ccNo) {
    return (
      from signalConnection in SignalConnections
      where signalConnection.CcNo == ccNo
      select signalConnection).FirstOrDefault();
  }

  public List<SignalConnection> GetForMacroSignalConnections() {
    return (
      from signalConnection in SignalConnections
      where signalConnection.IsForMacro
      select signalConnection).ToList();
  }

  public override string ToString() {
    return $"{DisplayName} ({Name})";
  }
}