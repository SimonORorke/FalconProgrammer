using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   Called SignalConnection in the program XML but corresponds to a modulation,
///   as in the user interface and manual. Among other things, this can map a macro to
///   a MIDI CC number.
/// </summary>
public class Modulation : EntityBase {
  public Modulation(ProgramXml programXml) : base(programXml, true) {
    Ratio = 1;
    Source = string.Empty;
    Destination = "Value";
    ConnectionMode = 1;
  }

  public Modulation(EntityBase owner, XElement modulationElement, ProgramXml programXml, 
    MidiForMacros midi) : base(programXml) {
    Owner = owner;
    Element = modulationElement;
    SubstituteCcNoForPlaceholder(midi);
  }

  public int? CcNo {
    get =>
      Source.StartsWith("@MIDI CC ")
        ? Convert.ToInt32(Source.Replace("@MIDI CC ", string.Empty))
        : null; // Effect modulated by macro
    set => Source = $"@MIDI CC {value}";
  }

  /// <summary>
  ///   Gets whether the MIDI CC mapping is to modulate a macro on the Info page.
  ///   So far, the only CC mappings that are not for Info page controls are for the
  ///   modulation wheel (MIDI CC 1).  Also false for effect signal connections.
  /// </summary>
  public int ConnectionMode {
    get => Convert.ToInt32(GetAttributeValue(nameof(ConnectionMode)));
    set => SetAttribute(nameof(ConnectionMode), value);
  }

  /// <summary>
  ///   Indicates what is to be modulated.
  ///   If the <see cref="Modulation" /> belongs to the
  ///   <see cref="FalconProgram.GuiScriptProcessor" />, this will be the
  ///   name in the script of the macro to be modulated, like "Macro1".
  ///   If the <see cref="Modulation" /> belongs to the <see cref="Macro" /> to
  ///   be modulated, this will be "Value".
  /// </summary>
  /// <remarks>
  ///   UVI evidently only references macros by names like "Macro1" internally in
  ///   scripts. In the ConstantModulation definition of the macro, even in programs with
  ///   Info page layout script processors, the name is like "Macro 1".
  /// </remarks>
  public string Destination {
    get => GetAttributeValue(nameof(Destination));
    set => SetAttribute(nameof(Destination), value);
  }

  public EntityBase? Owner { get; set; }

  public float Ratio {
    get => Convert.ToSingle(GetAttributeValue(nameof(Ratio)));
    set => SetAttribute(nameof(Ratio), value);
  }

  /// <summary>
  ///   The MIDI CC number that is the source of the modulation,
  ///   like this: '"@MIDI CC n"'.
  ///   Or the path of the macro that modulates an effect.
  /// </summary>
  public string Source {
    get => GetAttributeValue(nameof(Source));
    set => SetAttribute(nameof(Source), value);
  }

  /// <summary>
  ///   If the <see cref="Modulation" /> belongs to an effect or the
  ///   <see cref="FalconProgram.GuiScriptProcessor" />, returns the
  ///   number (derived from<see cref="Macro.Name" />) of the macro to be modulated.
  ///   If the <see cref="Modulation" /> belongs to the <see cref="Macro" /> to
  ///   be modulated, returns null.
  /// </summary>
  private int? ModulatedMacroNo =>
    Destination.StartsWith("Macro")
      ? Convert.ToInt32(Destination.Replace("Macro", string.Empty))
      : null;

  /// <summary>
  ///   Gets whether the MIDI CC mapping is to modulate a macro on the Info page.
  /// </summary>
  public bool ModulatesMacro {
    get {
      return Owner switch {
        ConnectionsParent => false, // Includes derived type Effect
        Macro => ConnectionMode == 1, // 0 for modulation wheel (MIDI CC 1)
        ScriptProcessor => ModulatedMacroNo.HasValue,
        null => throw new InvalidOperationException(
          "Modulation.ModulatesMacro cannot be determined because " +
          "Owner has not been specified."),
        _ => throw new NotSupportedException(
          "Modulation.ModulatesMacro cannot be determined because " +
          $"Owner is of unsupported type {Owner!.GetType().Name}.")
      };
    }
  }

  public Macro? SourceMacro { get; set; }

  protected override XElement CreateElementFromTemplate() {
    return new XElement(ProgramXml.TemplateModulationElement);
  }
  
  protected override XElement GetElement() {
    var result = CreateElementFromTemplate();
    return result;
  }

  private void SubstituteCcNoForPlaceholder(MidiForMacros midi) {
    if (Source.StartsWith("@MIDI CC C")) {
      Source = GetSourceWithCcNo('C', midi.ContinuousCcNos);
    } else if (Source.StartsWith("@MIDI CC T")) {
      Source = GetSourceWithCcNo('T', midi.ToggleCcNos);
    }
    return;

    string GetSourceWithCcNo(char placeholderPrefix, IList<int> ccNos) {
      string placeholder = string.Empty;
      try {
        placeholder =
          Source.Replace($"@MIDI CC {placeholderPrefix}", string.Empty);
        int index = Convert.ToInt32(placeholder) - 1;
        int ccNo;
        if (index < ccNos.Count) {
          ccNo = ccNos[index];
        } else {
          int shortfall = index - ccNos.Count + 1;
          int maxSpecifiedCcNo = ccNos[^1];
          ccNo = maxSpecifiedCcNo + shortfall;
        }
        return $"@MIDI CC {ccNo}";
      } catch {
        throw new ApplicationException(
          $"Source '{Source}' contains invalid MIDI CC number index " + 
          $"'{placeholder}'. A positive integer is expected.");
      }
    }
  }
}