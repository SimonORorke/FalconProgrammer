using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Xml.Serialization;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlDeserialised;

/// <summary>
///   Called ConstantModulation in the program XML but corresponds to a macro,
///   as shown the Info page.
/// </summary>
public class Macro : INamed {
  private XElement? _macroElement;
  private XElement? _propertiesElement;

  /// <summary>
  ///   The macro name, which uniquely identifies the macro. For reference in
  ///   <see cref="Modulation" />s owned by effects or the
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
  ///   For a macro (ConstantModulation), there is 0 or 1 Modulation only, except
  ///   where there is a Modulation that maps to the modulation wheel (MIDI CC 1),
  ///   in which case there can be two Modulations.
  /// </summary>
  [XmlArray("Connections")]
  [XmlArrayItem("SignalConnection")]
  public List<Modulation> Modulations { get; set; } = null!;

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
  
  private XElement MacroElement {
    get => _macroElement ??= GetMacroElement();
    set => _macroElement = value;
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

  internal List<ConnectionsParent> ModulatedConnectionsParents { get; } = 
    new List<ConnectionsParent>();

  public bool ModulatesDelay => DisplayName.Contains("Delay");

  public bool ModulatesEnabledEffects => (
    from effect in ModulatedConnectionsParents
    where !effect.Bypass
    select effect).Any();

  public bool ModulatesReverb =>
    DisplayName.Contains("Reverb")
    || DisplayName.Contains("Room")
    || DisplayName.Contains("Verb");

  internal ProgramXml ProgramXml { get; set; } = null!;

  private XElement PropertiesElement => _propertiesElement ??= GetPropertiesElement();
  
  public void AddMacroElement() {
    MacroElement = new XElement(ProgramXml.TemplateMacroElement);
    // Remove any Modulation elements and their parent Connections element brought
    // over with template.
    MacroElement.Element("Connections")?.Remove();
    ProgramXml.SetAttribute(MacroElement, nameof(Name), Name);
    ProgramXml.SetAttribute(MacroElement, nameof(DisplayName), DisplayName);
    ProgramXml.SetAttribute(MacroElement, nameof(Bipolar), Bipolar);
    ProgramXml.SetAttribute(MacroElement, nameof(Style), Style);
    ProgramXml.SetAttribute(MacroElement, nameof(Value), Value);
    ProgramXml.ControlSignalSourcesElement.Add(MacroElement);
    // A macro is expected to own 0, 1 or 2 Modulations:
    // 2 if there is a mod wheel signal connection and a 'for macro' Modulation.
    foreach (var modulation in Modulations) {
      AddModulationElement(modulation);
    }
    UpdatePropertiesElement();
  }

  /// <summary>
  ///   Adds the specified <see cref="Modulation" /> to the <see cref="Macro" />
  ///   in the Linq For XML data structure as well as in the deserialised data structure.
  /// </summary>
  public void AddModulation(Modulation modulation) {
    modulation.Owner = this;
    Modulations.Add(modulation);
    AddModulationElement(modulation);
  }

  private void AddModulationElement(Modulation modulation) {
    // If there's already a modulation wheel assignment, the macro ("ConstantModulation")
    // element will already own a Connections element. 
    var connectionsElement = MacroElement.Element("Connections");
    if (connectionsElement == null) {
      connectionsElement = new XElement("Connections");
      MacroElement.Add(connectionsElement);
    }
    connectionsElement.Add(ProgramXml.CreateModulationElement(modulation));
  }

  public void ChangeCcNoTo(int newCcNo) {
    var forMacroModulation = GetForMacroModulations().FirstOrDefault();
    if (forMacroModulation != null) {
      if (newCcNo != forMacroModulation.CcNo) {
        forMacroModulation.CcNo = newCcNo;
        UpdateModulation(forMacroModulation);
      }
    }
  }

  /// <summary>
  ///   Change all effect parameters that are currently modulated by the modulation wheel
  ///   to be modulated by the specified macro instead.
  /// </summary>
  public void ChangeModWheelModulationSourcesToMacro() {
    string newSource = $"$Program/{Name}";
    var modWheelModulationElements = 
      ProgramXml.GetModulationElementsWithCcNo(1);
    foreach (var modulationElement in modWheelModulationElements) {
      ProgramXml.SetAttribute(
        modulationElement, nameof(Modulation.Source), newSource);
    }
  }

  public void ChangeValueToZero() {
    ProgramXml.SetAttribute(MacroElement, nameof(Value), 0);
    foreach (var effect in ModulatedConnectionsParents) {
      effect.ChangeModulatedParametersToZero();
    }
  }

  public Modulation? FindModulationWithCcNo(int ccNo) {
    return (
      from modulation in Modulations
      where modulation.CcNo == ccNo
      select modulation).FirstOrDefault();
  }

  [SuppressMessage("ReSharper", "CommentTypo")]
  public List<Modulation> GetForMacroModulations() {
    return (
      from modulation in Modulations
      where modulation.ModulatesMacro
      select modulation).ToList();
  }

  private XElement GetMacroElement() {
    var result = (
      from macroElement in ProgramXml.MacroElements
      // Match on Name, not DisplayName. There can be duplicate DisplayNames.
      // Example: Devinity\Bass\Talking Bass, which has three toggle macros with blank
      // DisplayNames.
      where ProgramXml.GetAttributeValue(macroElement, nameof(Name)) == Name
      // where string.Equals(ProgramXml.GetAttributeValue(
      // macroElement, nameof(DisplayName)), DisplayName, StringComparison.OrdinalIgnoreCase) 
      select macroElement).FirstOrDefault();
    if (result != null) {
      return result;
    }
    throw new InvalidOperationException(
      $"Cannot find ConstantModulation '{Name}' in " + 
      $"'{ProgramXml.InputProgramPath}'.");
  }

  private XElement GetPropertiesElement() {
    var result = MacroElement.Element("Properties");
    if (result == null) {
      throw new InvalidOperationException(
        "Cannot find ConstantModulation.Properties "
        + $"element in '{ProgramXml.Category.TemplateProgramPath}'.");
    }
    return result;
  }

  public void RemoveMacroElement() {
    MacroElement.Remove();
    for (int i = ModulatedConnectionsParents.Count - 1; i >= 0; i--) {
      ModulatedConnectionsParents[i].RemoveModulationsByMacro(this);
      ModulatedConnectionsParents.RemoveAt(i);
    }
  }

  /// <summary>
  ///   Removes the specified <see cref="Modulation" /> from the Linq For XML data
  ///   structure as well as from the <see cref="Macro" /> in the deserialised data
  ///   structure.
  /// </summary>
  public void RemoveModulation(Modulation modulation) {
    if (Modulations.Contains(modulation)) {
      Modulations.Remove(modulation);
    }
    ProgramXml.RemoveModulationElementsWithCcNo(modulation.CcNo!.Value);
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

  public void UpdateModulation(Modulation modulation) {
    var connectionsElement = MacroElement.Element("Connections")!;
    var modulationElements =
      connectionsElement.Elements("SignalConnection").ToList();
    // The macro ("ConstantModulation") will have two Modulations if one of them
    // maps to the modulation wheel (MIDI CC 1). 
    var modulationElement = modulationElements[modulation.Index];
    ProgramXml.UpdateModulationElement(modulation, modulationElement);
  }
  
  public void UpdateLocation() {
    UpdatePropertiesElement();
  }
}