using System.Collections.Immutable;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlLinq;

/// <summary>
///   Called ConstantModulation in the program XML but corresponds to a macro,
///   as shown the Info page.
/// </summary>
public class Macro : ModulationsOwnerBase {
  private const int FirstContinuousCcNo = 31;
  private const int FirstToggleCcNo = 112;
  private XElement? _propertiesElement;
  public Macro(ProgramXml programXml) : base(programXml, true) { }

  public Macro(XElement macroElement, ProgramXml programXml) : base(programXml) {
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

  public double Value {
    get => Convert.ToDouble(GetAttributeValue(nameof(Value)));
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

  public void ChangeCcNoTo(int newCcNo) {
    // Convert MIDI CC 38, which does not work with macros on script-based Info
    // pages, to 28.
    int targetCcNo = newCcNo != 38 ? newCcNo : 28;
    var forMacroModulation = GetForMacroModulations().FirstOrDefault();
    if (forMacroModulation != null) {
      if (targetCcNo != forMacroModulation.CcNo) {
        forMacroModulation.CcNo = targetCcNo;
        // forMacroModulation.Update();
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
      select macroElement).FirstOrDefault();
    if (result != null) {
      return result;
    }
    throw new InvalidOperationException(
      $"Cannot find ConstantModulation '{Name}' in " +
      $"'{ProgramXml.InputProgramPath}'.");
  }

  public ImmutableList<Modulation> GetForMacroModulations() {
    return (
      from modulation in Modulations
      where modulation.ModulatesMacro
      select modulation).ToImmutableList();
  }
  
  public int GetNextCcNo(ref int continuousCcNo, ref int toggleCcNo, 
    bool reuseCc1) {
    if (!IsContinuous) {
      // Map button CC to toggle macro. 
      if (toggleCcNo < FirstToggleCcNo) {
        toggleCcNo = FirstToggleCcNo;
      } else {
        toggleCcNo++;
      }
      return toggleCcNo;
    }
    // Map continuous controller CC to continuous macro.
    switch (continuousCcNo) {
      case 1: // Wheel
        continuousCcNo = 11; // Touch strip
        break;
      case 11: // Touch strip
        continuousCcNo = 36; 
        break;
      case 28:
        continuousCcNo = 41; // Start of knob bank 2 
        break;
      case < FirstContinuousCcNo: // e.g. 0
        continuousCcNo = FirstContinuousCcNo; // 31
        break;
      case <= FirstContinuousCcNo + 2: // 31-33
        continuousCcNo++;
        break;
      case FirstContinuousCcNo + 3: // 34
        continuousCcNo = reuseCc1 ? 1 : 11; // Wheel or touch strip
        break;
      case 37: 
        // MIDI CC 38 does not work with macros on script-based Info pages
        continuousCcNo = 28; 
        break;
      case 48:
        continuousCcNo = 51; // Start of knob bank 3 
        break;
      case 58:
        continuousCcNo = 61; // Start of knob bank 4 
        break;
      default:
        continuousCcNo++;
        break;
    }
    return continuousCcNo;
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

  public override string ToString() {
    return $"{DisplayName} ({Name})";
  }
}