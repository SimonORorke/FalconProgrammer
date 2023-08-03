﻿using System.Xml.Linq;
using System.Xml.Serialization;
using FalconProgrammer.XmlLinq;

namespace FalconProgrammer.XmlDeserialised;

/// <summary>
///   Among other things, this can map a macro to a MIDI CC number. A better name might
///   be Modulation.
/// </summary>
public class SignalConnection {
  // private Macro? _destinationMacro;

  public SignalConnection() {
    Ratio = 1;
    Source = string.Empty;
    Destination = "Value";
    ConnectionMode = 1;
  }

  public SignalConnection(INamed owner, XElement signalConnectionElement) {
    Owner = owner;
    SignalConnectionElement = signalConnectionElement;
    var ratioAttribute =
      SignalConnectionElement.Attribute(nameof(Ratio)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.Ratio attribute.");
    Ratio = Convert.ToSingle(ratioAttribute.Value);
    var sourceAttribute =
      SignalConnectionElement.Attribute(nameof(Source)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.Source attribute.");
    Source = sourceAttribute.Value;
    var destinationAttribute =
      SignalConnectionElement.Attribute(nameof(Destination)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.Destination attribute.");
    Destination = destinationAttribute.Value;
    var connectionModeAttribute =
      SignalConnectionElement.Attribute(nameof(ConnectionMode)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.ConnectionMode attribute.");
    ConnectionMode = Convert.ToInt32(connectionModeAttribute.Value);
  }

  [XmlAttribute] public float Ratio { get; set; }

  /// <summary>
  ///   The MIDI CC number that is the source of the modulation,
  ///   like this: '"@MIDI CC n"'.
  ///   Or the path of the macro that modulates an effect.
  /// </summary>
  [XmlAttribute]
  public string Source { get; set; }

  /// <summary>
  ///   Indicates what is to be modulated.
  ///   If the <see cref="SignalConnection" /> belongs to the
  ///   <see cref="FalconProgram.InfoPageCcsScriptProcessor" />, this will be the
  ///   name in the script of the macro to be modulated, like "Macro1".
  ///   If the <see cref="SignalConnection" /> belongs to the <see cref="Macro" /> to
  ///   be modulated, this will be "Value".
  /// </summary>
  /// <remarks>
  ///   UVI evidently only reference macros by names like "Macro1" internally in scripts.
  ///   In the ConstantModulation definition of the macro, even in programs with
  ///   Info page layout script processors, the name is like "Macro 1".
  /// </remarks>
  [XmlAttribute]
  public string Destination { get; set; }

  /// <summary>
  ///   Gets whether the MIDI CC mapping is to modulate a macro on the Info page.
  ///   So far, the only CC mappings that are not for Info page controls are for the
  ///   modulation wheel (MIDI CC 1).  Also false for effect signal connections.
  /// </summary>
  [XmlAttribute]
  public int ConnectionMode { get; set; }

  public int? CcNo {
    get =>
      Source.StartsWith("@MIDI CC ")
        ? Convert.ToInt32(Source.Replace("@MIDI CC ", string.Empty))
        : null; // Effect modulated by macro
    set => Source = $"@MIDI CC {value}";
  }

  public int Index { get; set; }

  internal INamed? Owner { get; set; }

  /// <summary>
  ///   Gets whether the MIDI CC mapping is to modulate a macro on the Info page.
  /// </summary>
  public bool ModulatesMacro {
    get {
      return Owner switch {
        Macro => ConnectionMode == 1, // 0 for modulation wheel (MIDI CC 1)
        ScriptProcessor => ModulatedMacroNo.HasValue,
        Effect => false,
        null => throw new InvalidOperationException(
          "SignalConnection.ModulatesMacro cannot be determined because " +
          "Owner has not been specified."),
        _ => throw new NotSupportedException(
          "SignalConnectionModulatesMacro cannot be determined because " +
          $"Owner is of unsupported type {Owner.GetType().Name}.")
      };
    }
  }

  internal XElement? SignalConnectionElement { get; }
  public Macro? SourceMacro { get; set; }

  /// <summary>
  ///   If the <see cref="SignalConnection" /> belongs to an effect or the
  ///   <see cref="FalconProgram.InfoPageCcsScriptProcessor" />, returns the
  ///   number (derived from<see cref="Macro.Name" />) of the macro to be modulated.
  ///   If the <see cref="SignalConnection" /> belongs to the <see cref="Macro" /> to
  ///   be modulated, returns null.
  /// </summary>
  private int? ModulatedMacroNo =>
    Destination.StartsWith("Macro")
      ? Convert.ToInt32(Destination.Replace("Macro", string.Empty))
      : null;
}