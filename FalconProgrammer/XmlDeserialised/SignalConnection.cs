using System.Xml.Linq;
using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised;

/// <summary>
///   Among other things, this can map a macro to a MIDI CC number. A better name might
///   be Modulation.
/// </summary>
public class SignalConnection {
  
  public SignalConnection() {
    Ratio = 1;
    Source = string.Empty;
    Destination = "Value";
    ConnectionMode = 1;
  }

  public SignalConnection(XElement signalConnectionElement) {
    var ratioAttribute =
      signalConnectionElement.Attribute(nameof(Ratio)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.Ratio attribute.");
    Ratio = Convert.ToSingle(ratioAttribute.Value);
    var sourceAttribute =
      signalConnectionElement.Attribute(nameof(Source)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.Source attribute.");
    Source = sourceAttribute.Value;
    var destinationAttribute =
      signalConnectionElement.Attribute(nameof(Destination)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.Destination attribute.");
    Destination = destinationAttribute.Value;
    var connectionModeAttribute =
      signalConnectionElement.Attribute(nameof(ConnectionMode)) ??
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
  [XmlAttribute] public string Source { get; set; }
  
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
  [XmlAttribute] public string Destination { get; set; }
  [XmlAttribute] public int ConnectionMode { get; set; }

  public int? CcNo {
    get =>
      Source.StartsWith("@MIDI CC ")
        ? Convert.ToInt32(Source.Replace("@MIDI CC ", string.Empty))
        : null; // Effect modulated by macro
    set => Source = $"@MIDI CC {value}";
  }

  public int Index { get; set; }

  /// <summary>
  ///   Gets whether the MIDI CC mapping is to control a macro on the Info page.
  ///   So far, the only CC mappings that are not for Info page controls are for the
  ///   modulation wheel (MIDI CC 1).  Also false for effect signal connections.
  /// </summary>
  public bool IsForMacro => ConnectionMode == 1;

  /// <summary>
  ///   If the <see cref="SignalConnection" /> belongs to and effect or the
  ///   <see cref="FalconProgram.InfoPageCcsScriptProcessor" />, returns the
  ///   number (derived from<see cref="Macro.Name" />) of the macro to be modulated.
  ///   If the <see cref="SignalConnection" /> belongs to the <see cref="Macro" /> to
  ///   be modulated, returns null. 
  /// </summary>
  /// <exception cref="ApplicationException">
  ///   Thrown when an attempt is made to set <see cref="MacroNo" /> for a
  ///   <see cref="SignalConnection" /> that is owned by the modulated
  ///   <see cref="Macro" />.
  /// </exception>
  public int? MacroNo {
    get =>
      Destination.StartsWith("Macro")
        ? Convert.ToInt32(Destination.Replace("Macro", string.Empty))
        : null;
    set {
      if (Destination == "Value") {
        throw new ApplicationException(
          "MacroNo may not be set for a SignalConnection that is owned by the " +
          "modulated macro (SignalConnection.Destination = 'Value'), only for a " +
          "SignalConnection that is owned by effects or the " +
          "FalconProgram.InfoPageCcsScriptProcessor.");
      }
      Destination = $"Macro{value}";
    }
  }
}