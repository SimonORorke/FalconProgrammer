﻿using System.Collections.Immutable;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlLinq; 

/// <summary>
///   Called ConstantModulation in the program XML but corresponds to a macro,
///   as shown the Info page.
/// </summary>
public class Macro : EntityBase {
  // private XElement? _connectionsElement;
  private ImmutableList<Modulation>? _modulations;
  private XElement? _propertiesElement;

  public Macro(ProgramXml programXml) : base(programXml, true) {
  }

  public Macro(XElement macroElement, ProgramXml programXml) : base(programXml) {
    Element = macroElement;
  }

  public int Bipolar {
    get => Convert.ToInt32(GetAttributeValue(nameof(Bipolar)));
    set => SetAttribute(nameof(Bipolar), value);
  }
  
  // private XElement ConnectionsElement => _connectionsElement ??= GetConnectionsElement();

  /// <summary>
  ///   The meaningful name of the macro, as displayed on the Info page.
  /// </summary>
  public string DisplayName {
    get => GetAttributeValue(nameof(DisplayName));
    set => SetAttribute(nameof(DisplayName), value);
  }

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
  ///   The macro number, derived from <see cref="EntityBase.Name" />.
  /// </summary>
  /// <remarks>
  ///   Though <see cref="EntityBase.Name" /> is a unique identifier, this derived macro
  ///   number cannot be 100% relied on to also be one. This is because there is at least
  ///   one program, Titanium\Pads\Children's Choir, that has macros named both "Macro 1"
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
  
  public List<ConnectionsParent> ModulatedConnectionsParents { get; } =
    new List<ConnectionsParent>();

  /// <summary>
  ///   Modulations specifying MIDI CC numbers that modulate the macro.
  /// </summary>
  /// <remarks>
  ///   For a macro (ConstantModulation), there is 0 or 1 Modulation only, except
  ///   where there is a Modulation that maps to the modulation wheel (MIDI CC 1),
  ///   in which case there can be two Modulations.
  /// </remarks>
  public ImmutableList<Modulation> Modulations {
    get => _modulations ??= GetModulations();
    private set => _modulations = value;
  }
  public bool ModulatesDelay => DisplayName.Contains("Delay");

  public bool ModulatesEnabledEffects => (
    from effect in ModulatedConnectionsParents
    where !effect.Bypass
    select effect).Any();

  public bool ModulatesReverb =>
    DisplayName.Contains("Reverb")
    || DisplayName.Contains("Room")
    // ReSharper disable once StringLiteralTypo
    || DisplayName.Contains("Sparkverb") // There's at least one like this.
    || DisplayName.Contains("Verb");
  
  private XElement PropertiesElement => _propertiesElement ??= GetPropertiesElement();

  /// <summary>
  ///   0 indicates a toggle macro. 1 indicates a continuous macro.
  /// </summary>
  public int Style {
    get => Convert.ToInt32(GetAttributeValue(nameof(Style)));
    set => SetAttribute(nameof(Style), value);
  }

  public float Value {
    get => Convert.ToSingle(GetAttributeValue(nameof(Value)));
    set => SetAttribute(nameof(Value), value);
  }
  
  public int X {
    get => Convert.ToInt32(GetAttributeValue(
      PropertiesElement, nameof(X).ToLower()));
    set => SetAttribute(
      PropertiesElement, nameof(X).ToLower(), value);
  }
  
  public int Y {
    get => Convert.ToInt32(GetAttributeValue(
      PropertiesElement, nameof(Y).ToLower()));
    set => SetAttribute(
      PropertiesElement, nameof(Y).ToLower(), value);
  }

  public void AddElement() {
    // Remove any Modulation elements and their parent Connections element brought
    // over with template.
    Element.Element("Connections")?.Remove();
    SetAttribute(nameof(Name), Name);
    SetAttribute(nameof(DisplayName), DisplayName);
    SetAttribute(nameof(Bipolar), Bipolar);
    SetAttribute(nameof(Style), Style);
    SetAttribute(nameof(Value), Value);
    // A macro is expected to own 0, 1 or 2 Modulations:
    // 2 if there is a mod wheel signal connection and a 'for macro' Modulation.
    if (Modulations.Count > 0) {
      var connectionsElement = GetConnectionsElement();
      foreach (var modulation in Modulations) {
        connectionsElement.Add(modulation.Element);
      }
    }
    UpdatePropertiesElement();
  }
  
  public void AddModulation(Modulation modulation) {
    modulation.Owner = this;
    GetConnectionsElement().Add(modulation.Element);
    Modulations = Modulations.Add(modulation);
  }

  public void ChangeCcNoTo(int newCcNo) {
    // Convert MIDI CC 38, which does not work with macros on script-based Info
    // pages, to 28.
    int targetCcNo = newCcNo != 38 ? newCcNo : 28;
    var forMacroModulation = GetForMacroModulations().FirstOrDefault();
    if (forMacroModulation != null) {
      if (targetCcNo != forMacroModulation.CcNo) {
        forMacroModulation.CcNo = targetCcNo;
        forMacroModulation.Update();
      }
    } else {
      // ReSharper disable once CommentTypo
      // Example: Reverb Mix macro of Factory\Polysynth\Velocity Pluck 
      AddModulation(new Modulation(ProgramXml) {
        CcNo = targetCcNo
      });
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
      SetAttribute(
        modulationElement, nameof(Modulation.Source), newSource);
    }
  }

  public void ChangeValueToZero() {
    Value = 0;
    foreach (var effect in ModulatedConnectionsParents) {
      effect.ChangeModulatedParametersToZero();
    }
  }

  protected override XElement CreateElementFromTemplate() {
    var result = new XElement(ProgramXml.TemplateMacroElement);
    // Remove any modulations (SignalConnections) that might have been copied over with
    // the template.
    result.Element("Connections")?.Remove();
    ProgramXml.ControlSignalSourcesElement.Add(result);
    return result;
  }

  public Modulation? FindModulationWithCcNo(int ccNo) {
    return (
      from modulation in Modulations
      where modulation.CcNo == ccNo
      select modulation).FirstOrDefault();
  }

  protected override XElement GetElement() {
    var result = (
      from macroElement in ProgramXml.MacroElements
      // Match on Name, not DisplayName. There can be duplicate DisplayNames.
      // Example: Devinity\Bass\Talking Bass, which has three toggle macros with blank
      // DisplayNames.
      where GetAttributeValue(macroElement, nameof(Name)) == Name
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

  private XElement GetConnectionsElement() {
    // If there's already a modulation wheel assignment, the macro ("ConstantModulation")
    // element will already own a Connections element. 
    var result = Element.Element("Connections");
    if (result == null) {
      result = new XElement("Connections");
      Element.Add(result);
    }
    return result;
  }
  
  public ImmutableList<Modulation> GetForMacroModulations() {
    return (
      from modulation in Modulations
      where modulation.ModulatesMacro
      select modulation).ToImmutableList();
  }

  private ImmutableList<Modulation> GetModulations() {
    var list = new List<Modulation>();
    var connectionsElement = Element.Element("Connections");
    if (connectionsElement != null) {
      list.AddRange(connectionsElement.Elements("SignalConnection").Select(
        modulationElement => new Modulation(
          this, modulationElement, ProgramXml)));
    }
    return list.ToImmutableList();
  }

  private XElement GetPropertiesElement() {
    var result = Element.Element("Properties");
    if (result == null) {
      throw new InvalidOperationException(
        "Cannot find ConstantModulation.Properties "
        + $"element in '{ProgramXml.Category.TemplateProgramPath}'.");
    }
    return result;
  }

  public void RemoveElement() {
    Element.Remove();
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
      Modulations = Modulations.Remove(modulation);
    }
    ProgramXml.RemoveModulationElementsWithCcNo(modulation.CcNo!.Value);
  }

  public override string ToString() {
    return $"{DisplayName} ({Name})";
  }

  private void UpdatePropertiesElement() {
    // customPosition needs to be update if we are converting the layout from a
    // script processor layout to a standard layout.
    SetAttribute(PropertiesElement, "customPosition", 1);
    SetAttribute(PropertiesElement, "x", X);
    SetAttribute(PropertiesElement, "y", Y);
  }
}