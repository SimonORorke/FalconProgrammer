using System.Collections.Immutable;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   Called ConstantModulation in the program XML but corresponds to a macro,
///   as shown the Info page.
/// </summary>
internal class Macro : ModulationsOwner {
  private XElement? _propertiesElement;

  public Macro(ProgramXml programXml, MidiForMacros midi)
    : base(programXml, midi, true) { }

  public Macro(XElement macroElement, ProgramXml programXml, MidiForMacros midi)
    : base(programXml, midi) {
    Element = macroElement;
  }

  public int Bipolar {
    get => Convert.ToInt32(GetAttributeValue(nameof(Bipolar)));
    set => SetAttribute(nameof(Bipolar), value);
  }

  /// <summary>
  ///   For non-ScriptProcessor macros, unless this is true, X and Y are ignored and the
  ///   macro is given a default location.
  /// </summary>
  public bool CustomPosition {
    get => GetAttributeValue(PropertiesElement, "customPosition") == "1";
    set => SetAttribute(PropertiesElement,
      "customPosition", value ? "1" : "0");
  }

  public string DisplayNameWithoutCc => DisplayName.Contains(" (CC")
    ? DisplayName[..DisplayName.IndexOf(" (CC")].Trim()
    : DisplayName;

  /// <summary>
  ///   Gets or sets whether this is continuous macro.  If false, it's a toggle macro.
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
  ///   Gets whether this is toggle macro. If false, it's a continuous macro.
  /// </summary>
  public bool IsToggle => !IsContinuous;

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

  internal bool IsModulatedByWheel => FindModulationWithCcNo(1) != null;
  internal List<ConnectionsParent> ModulatedConnectionsParents { get; } = [];
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
  ///   0 indicates a continuous macro. 1 indicates a toggle macro.
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

  /// <summary>
  ///   This is currently only used by <see cref="FalconProgram.ReuseCc1" />.
  /// </summary>
  public void ChangeCcNoTo(int newCcNo) {
    // Convert MIDI CC 38, which does not work with macros on script-based Info
    // pages, to 28.
    int targetCcNo = newCcNo != 38 ? newCcNo : 28;
    var forMacroModulation = GetForMacroModulations().FirstOrDefault(); // ???
    if (forMacroModulation != null) {
      if (targetCcNo != forMacroModulation.CcNo) {
        forMacroModulation.CcNo = targetCcNo;
      }
    } else {
      // ReSharper disable once CommentTypo
      // Example: Reverb Mix macro of Falcon Factory\Polysynth\Velocity Pluck 
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
      select macroElement).FirstOrDefault();
    if (result != null) {
      return result;
    }
    throw new InvalidOperationException(
      $"Cannot find ConstantModulation '{Name}' in " +
      $"'{ProgramXml.InputProgramPath}'.");
  }

  /// <summary>
  ///   This is currently only used by <see cref="FalconProgram.ReuseCc1" />.
  /// </summary>
  public ImmutableList<Modulation> GetForMacroModulations() {
    return (
      from modulation in Modulations
      where modulation.ModulatesMacro
      select modulation).ToImmutableList();
  }

  private XElement GetPropertiesElement() {
    var result = Element.Element("Properties");
    if (result == null) {
      throw new InvalidOperationException(
        "Cannot find ConstantModulation.Properties element.");
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

  public override string ToString() {
    return $"{DisplayName} ({Name})";
  }
}