using System.Xml.Linq;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   Called SignalConnection in the program XML but corresponds to a modulation,
///   as in the user interface and manual. Among other things, this can map a macro to
///   a MIDI CC number.
/// </summary>
internal class Modulation : EntityBase {
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
    Midi = midi;
    SubstituteCcNoForPlaceholder();
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
  /// </summary>
  /// <remarks>
  ///   If the <see cref="Modulation" /> belongs to the <see cref="Macro" /> to
  ///   be modulated, this will be "Value".
  ///   <para>
  ///     If the <see cref="Modulation" /> belongs to the
  ///     <see cref="FalconProgram.GuiScriptProcessor" />, this will be the name of the
  ///     script parameter to be modulated. If the script parameter modulates a macro,
  ///     Destination may start with "Macro, like "Macro1" in most Falcon Factory
  ///     (version 1) categories and some other sound banks,
  ///     or "Macro_1" in Falcon Factory rev2. But this is not consistent across sound
  ///     banks. For Fluidity the Destination format is "Custom_n".
  ///     For Factory\Organic Texture 2.8, Organic Keys, Organic Pads, Pulsar and Voklm,
  ///     Destination indicates the modulation type and does not end with a number.
  ///   </para>
  ///   <para>
  ///     Furthermore, if the Destination of a <see cref="Modulation" /> owned by the
  ///     <see cref="FalconProgram.GuiScriptProcessor" /> does end with a number,
  ///     there is no guarantee that the number matches the number at the end of the
  ///     <see cref="Macro.Name" /> of the modulated <see cref="Macro" />.
  ///     For example, in Falcon Factory\Brutal Bass 2.1\808 Line and many other programs
  ///     in the same category, the Modulation with Destination "Macro4" modulates the
  ///     Macro with Name "Macro 3".
  ///   </para>
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
    set =>
      SetAttribute(nameof(Source), value);
  }

  private MidiForMacros? Midi { get; }

  /// <summary>
  ///   Gets whether the MIDI CC mapping is to modulate a macro on the Info page.
  /// </summary>
  /// <remarks>
  ///   This is currently only used by <see cref="FalconProgram.ReuseCc1" />.
  /// </remarks>
  public bool ModulatesMacro {
    get {
      return Owner switch {
        ConnectionsParent => false, // Includes derived type Effect
        Macro => ConnectionMode == 1, // 0 for modulation wheel (MIDI CC 1)
        // Not used, as ReuseCc1 does not support GUI script processor.
        ScriptProcessor => CcNo.HasValue,
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

  /// <summary>
  ///   Fixes the source MIDI CC number if it has been given a toggle CC number based on
  ///   the template but the destination macro is continuous.
  /// </summary>
  /// <param name="macros">All the program's macros.</param>
  /// <param name="modulations">All the GUI script processor's modulations.</param>
  /// <remarks>
  ///   Currently this only works for Falcon Factory\Brutal Bass 2.1.
  ///   Examples: Magnetic 1, Overdrive.
  /// </remarks>
  public void FixToggleOrContinuous(IEnumerable<Macro> macros,
    IList<Modulation> modulations) {
    // In many but not all programs in category Falcon Factory\Brutal Bass 2.1,
    // Destination does not match the name of the modulated macro. For the potentially
    // problematic modulation, Destination is "Macro4", while the macro Name is
    // "Macro 3"! Examples: 808 Line, Overdrive.
    // Some other programs in the category do have the consistent names, where
    // the macro Name is "Macro 4". Example: World Up.
    if (Destination != "Macro4") {
      return;
    }
    // Fortunately Destination "Macro4" always modulates the Step Arp macro, a toggle
    // macro, if it exists. If it does, there is no problem, as the template gives
    // the "Macro4" a toggle MIDI CC number.
    bool programHasStepArpMacro = (
      from macro in macros
      where macro.DisplayName == "Step Arp"
      select macro).Any();
    if (programHasStepArpMacro) {
      return;
    }
    int maxExistingContinuousCcNo = (
      from modulation in modulations
      where Midi!.ContinuousCcNos.Contains(modulation.CcNo!.Value)
      select modulation.CcNo!.Value).Max();
    Midi!.CurrentContinuousCcNo = maxExistingContinuousCcNo;
    int newCcNo = Midi.GetNextContinuousCcNo(false);
    Source = Source.Replace(CcNo!.Value.ToString(), newCcNo.ToString());
  }

  protected override XElement GetElement() {
    var result = CreateElementFromTemplate();
    return result;
  }

  private void SubstituteCcNoForPlaceholder() {
    if (Source.StartsWith("@MIDI CC C")) {
      Source = GetSourceWithCcNo('C', Midi!.ContinuousCcNos);
    } else if (Source.StartsWith("@MIDI CC T")) {
      Source = GetSourceWithCcNo('T', Midi!.ToggleCcNos);
    }
    return;

    string GetSourceWithCcNo(char placeholderPrefix, IList<int> ccNos) {
      string placeholderIndex = string.Empty;
      try {
        placeholderIndex =
          Source.Replace($"@MIDI CC {placeholderPrefix}", string.Empty);
        int index = Convert.ToInt32(placeholderIndex) - 1;
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
          $"Source '{Source}' contains a MIDI CC number placeholder with " +
          $"invalid index '{placeholderIndex}'. A positive integer is expected.");
      }
    }
  }
}