using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Xml.Serialization;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlDeserialised;

/// <summary>
///   Called ConstantModulation in the program XML but can only represent a macro, so far
///   as I can tell.
/// </summary>
public class Macro {
  private XElement? _macroElement;
  private XElement? _propertiesElement;

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
  
  private XElement MacroElement => _macroElement ??= GetMacroElement();

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

  internal List<Effect> ModulatedEffects { get; } = new List<Effect>();

  public bool ModulatesDelay => DisplayName.Contains("Delay");

  public bool ModulatesReverb =>
    DisplayName.Contains("Reverb")
    || DisplayName.Contains("Room")
    || DisplayName.Contains("Verb");

  internal ProgramXml ProgramXml { get; set; } = null!;

  private XElement PropertiesElement => _propertiesElement ??= GetPropertiesElement();
  
  public void AddMacroElement() {
    // Remove any SignalConnection elements and their parent Connections element brought
    // over with template.
    MacroElement.Element("Connections")?.Remove();
    ProgramXml.SetAttribute(MacroElement, nameof(Name), Name);
    ProgramXml.SetAttribute(MacroElement, nameof(DisplayName), DisplayName);
    ProgramXml.SetAttribute(MacroElement, nameof(Bipolar), Bipolar);
    ProgramXml.SetAttribute(MacroElement, nameof(Style), Style);
    ProgramXml.SetAttribute(MacroElement, nameof(Value), Value);
    ProgramXml.ControlSignalSourcesElement.Add(MacroElement);
    ProgramXml.MacroElements = ProgramXml.ControlSignalSourcesElement.Elements("ConstantModulation").ToList();
    // A macro is expected to own 0, 1 or 2 SignalConnections:
    // 2 if there is a mod wheel signal connection and a 'for macro' SignalConnection.
    foreach (var signalConnection in SignalConnections) {
      AddSignalConnection(signalConnection);
    }
    UpdatePropertiesElement();
  }

  /// <summary>
  ///   Adds the specified <see cref="SignalConnection" /> to the <see cref="Macro" />
  ///   in the Linq For XML data structure as well as in the deserialised data structure.
  /// </summary>
  public void AddSignalConnection(SignalConnection signalConnection) {
    SignalConnections.Add(signalConnection);
    // ProgramXml.AddMacroSignalConnection(signalConnection, this);
    // If there's already a modulation wheel assignment, the macro ("ConstantModulation")
    // element will already own a Connections element. 
    var connectionsElement = MacroElement.Element("Connections");
    if (connectionsElement == null) {
      connectionsElement = new XElement("Connections");
      MacroElement.Add(connectionsElement);
    }
    connectionsElement.Add(ProgramXml.CreateSignalConnectionElement(signalConnection));
  }

  /// <summary>
  ///   Change all effect parameters that are currently modulated by the modulation wheel
  ///   to be modulated by the specified macro instead.
  /// </summary>
  public void ChangeModWheelSignalConnectionSourcesToMacro() {
    string newSource = $"$Program/{Name}";
    var modWheelSignalConnectionElements = 
      ProgramXml.GetSignalConnectionElementsWithCcNo(1);
    foreach (var signalConnectionElement in modWheelSignalConnectionElements) {
      ProgramXml.SetAttribute(
        signalConnectionElement, nameof(SignalConnection.Source), newSource);
    }
  }

  public void ChangeValueToZero() {
  // public bool ChangeValueToZero() {
    // // Ignore case when checking whether there is a macro with that display name.  An
    // // example of where the cases of macro display names are non-standard is
    // // Factory\Pure Additive 2.0\Bass Starter.
    // var MacroElement = (
    //   from element in ProgramXml.MacroElements
    //   where string.Equals(ProgramXml.GetAttributeValue(element, nameof(DisplayName)),
    //     DisplayName, StringComparison.OrdinalIgnoreCase)
    //   select element).FirstOrDefault();
    // if (MacroElement == null) {
    //   return false;
    // }
    ProgramXml.SetAttribute(MacroElement, nameof(Value), 0);
    // if (!ProgramXml.ChangeMacroValueToZero(this)) {
    //   return false;
    // }
    // Change the values of the effect parameters modulated by the macro as required too.
    foreach (var effect in ModulatedEffects) {
      effect.ChangeModulatedParametersToZero();
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
    // return true;
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
      where signalConnection.ModulatesMacro
      select signalConnection).ToList();
  }

  private XElement GetMacroElement() {
    // Ignore case when checking whether there is a macro with that display name.  An
    // example of where the cases of macro display names are non-standard is
    // Factory\Pure Additive 2.0\Bass Starter.
    var result = (from macroElement in ProgramXml.MacroElements
      where string.Equals(ProgramXml.GetAttributeValue(
        macroElement, nameof(DisplayName)), DisplayName, StringComparison.OrdinalIgnoreCase) 
        // macroElement, nameof(Name)) == Name
      select macroElement).FirstOrDefault();
    if (result != null) {
      return result;
    }
    throw new ApplicationException(
      $"Cannot find ConstantModulation '{Name}' in " + 
      $"'{ProgramXml.InputProgramPath}'.");
  }

  private XElement GetPropertiesElement() {
    var result = MacroElement.Element("Properties");
    if (result == null) {
      throw new ApplicationException(
        "Cannot find ConstantModulation.Properties "
        + $"element in '{ProgramXml.Category.TemplateProgramPath}'.");
    }
    return result;
  }

  public void RemoveDelayModulations() {
    for (int i = ModulatedEffects.Count - 1; i >= 0; i--) {
      if (ModulatedEffects[i].IsDelay) {
        ModulatedEffects[i].RemoveModulationsByMacro(this);
        ModulatedEffects.RemoveAt(i);
      }
    }
  }

  public void RemoveMacroElement() {
    MacroElement.Remove();
    ProgramXml.MacroElements = 
      ProgramXml.ControlSignalSourcesElement.Elements("ConstantModulation").ToList();
    for (int i = ModulatedEffects.Count - 1; i >= 0; i--) {
      ModulatedEffects[i].RemoveModulationsByMacro(this);
      ModulatedEffects.RemoveAt(i);
    }
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

  private void UpdatePropertiesElement() {
    // customPosition needs to be update if we are converting the layout from a
    // script processor layout to a standard layout.
    ProgramXml.SetAttribute(PropertiesElement, "customPosition", 1);
    ProgramXml.SetAttribute(PropertiesElement, "x", Properties.X);
    ProgramXml.SetAttribute(PropertiesElement, "y", Properties.Y);
  }

  public void UpdateSignalConnection(SignalConnection signalConnection) {
    var connectionsElement = MacroElement.Element("Connections")!;
    var signalConnectionElements =
      connectionsElement.Elements("SignalConnection").ToList();
    // The macro ("ConstantModulation") will have two SignalConnections if one of them
    // maps to the modulation wheel (MIDI CC 1). 
    var signalConnectionElement = signalConnectionElements[signalConnection.Index];
    ProgramXml.UpdateSignalConnectionElement(signalConnection, signalConnectionElement);
  }
  
  public void UpdateLocation() {
    UpdatePropertiesElement();
  }
}