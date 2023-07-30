using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlDeserialised;

/// <summary>
///   Called ConstantModulation in the program XML but can only represent a macro, so far
///   as I can tell.
/// </summary>
public class Macro {
  private ImmutableList<Effect>? _modulatedEffects;

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

  [XmlElement] public MacroProperties Properties { get; set; } = null!;

  /// <summary>
  ///   Indicates whether this is continuous macro.  If false, it's a toggle macro.
  /// </summary>
  public bool IsContinuous {
    get =>
      Style == 0 || (Style == 1
        ? false
        : throw new NotSupportedException(
          $"{nameof(Macro)}: {nameof(Style)} {Style} is not supported."));
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
          $"{nameof(Macro)}: '{Name}' is not a supported macro name.");
      }
      return macroNo;
    }
    set => Name = $"Macro {value}";
  }

  internal ImmutableList<Effect> ModulatedEffects =>
    _modulatedEffects ??= GetModulatedEffects();

  public bool ModulatesDelay => DisplayName.Contains("Delay");

  public bool ModulatesReverb =>
    DisplayName.Contains("Reverb")
    || DisplayName.Contains("Room")
    || DisplayName.Contains("Verb");

  internal ProgramXml ProgramXml { get; set; } = null!;

  /// <summary>
  ///   Adds the specified <see cref="SignalConnection" /> to the <see cref="Macro" />
  ///   in the Linq For XML data structure as well as in the deserialised data structure.
  /// </summary>
  public void AddSignalConnection(SignalConnection signalConnection) {
    SignalConnections.Add(signalConnection);
    ProgramXml.AddMacroSignalConnection(signalConnection, this);
  }

  public bool ChangeValueToZero() {
    if (!ProgramXml.ChangeMacroValueToZero(this)) {
      return false;
    }
    // Change the values of the effect parameters modulated by the macro as required too.
    foreach (var effect in ModulatedEffects) {
      effect.ChangeModulatedParameterToZero();
    }
    // var signalConnectionElementsWithMacroSource =
    //   ProgramXml.GetSignalConnectionElementsModulatedByMacro(this);
    // foreach (var signalConnectionElement in signalConnectionElementsWithMacroSource) {
    //   var connectionsElement = ProgramXml.GetParentElement(signalConnectionElement);
    //   var effectElement = ProgramXml.GetParentElement(connectionsElement);
    //   var signalConnection = new SignalConnection(signalConnectionElement);
    //   try {
    //     ProgramXml.SetAttribute(
    //       effectElement, signalConnection.Destination,
    //       // If it's a toggle macro, Destination should be "Bypass".  
    //       signalConnection.Destination == "Bypass" ? 1 : 0);
    //     // ReSharper disable once EmptyGeneralCatchClause
    //   } catch { }
    // }
    return true;
  }

  public SignalConnection? FindSignalConnectionWithCcNo(int ccNo) {
    return (
      from signalConnection in SignalConnections
      where signalConnection.CcNo == ccNo
      select signalConnection).FirstOrDefault();
  }

  [SuppressMessage("ReSharper", "CommentTypo")]
  public List<SignalConnection> GetForMacroSignalConnections() {
    return (
      from signalConnection in SignalConnections
      where signalConnection.IsForMacro
      // where signalConnection.IsForMacro && signalConnection.CcNo.HasValue
      select signalConnection).ToList();
  }

  private ImmutableList<Effect> GetModulatedEffects() {
    return ProgramXml.GetSignalConnectionElementsModulatedByMacro(this)
      .Select(signalConnectionElement => new Effect(signalConnectionElement, ProgramXml))
      .ToImmutableList();
  }

  /// <summary>
  ///   Removes the specified <see cref="SignalConnection" /> from the Linq For XML data
  ///   structure as well as from the <see cref="Macro" /> in the deserialised data
  ///   structure.
  /// </summary>
  public void RemoveSignalConnection(SignalConnection signalConnection) {
    if (SignalConnections.Contains(signalConnection)) {
      SignalConnections.Remove(signalConnection);
    }
    ProgramXml.RemoveSignalConnectionElementsWithCcNo(signalConnection.CcNo!.Value);
  }

  public override string ToString() {
    return $"{DisplayName} ({Name})";
  }
}