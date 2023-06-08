using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class FalconProgram {
  private List<Macro>? _continuousMacros;

  public FalconProgram(string path, Category category) {
    Path = path;
    Category = category;
  }

  [PublicAPI] public Category Category { get; }
  internal List<Macro> ContinuousMacros => _continuousMacros ??= GetContinuousMacros();
  internal ScriptProcessor? InfoPageCcsScriptProcessor { get; private set; }
  internal List<Macro> Macros { get; private set; } = null!;

  /// <summary>
  ///   Gets the order in which MIDI CC numbers are to be mapped to macros
  ///   relative to their locations on the Info page.
  /// </summary>
  private LocationOrder MacroCcLocationOrder { get; set; }

  internal string Name { get; private set; } = null!;
  private int NextContinuousCcNo { get; set; } = 31;
  private int NextToggleCcNo { get; set; } = 112;
  [PublicAPI] public string Path { get; }
  internal ProgramXml ProgramXml { get; private set; } = null!;
  private List<ScriptProcessor> ScriptProcessors { get; set; } = null!;

  public void ChangeDelayToZero() {
    foreach (var continuousMacro in ContinuousMacros.Where(
               continuousMacro =>
                 continuousMacro.ControlsDelay
                 && ChangeMacroValueToZero(continuousMacro.DisplayName))) {
      Console.WriteLine($"Changed {continuousMacro.DisplayName} to zero for '{Path}'.");
    }
  }

  private bool ChangeMacroValueToZero(string displayName) {
    string? macroName = ProgramXml.ChangeMacroValueToZero(displayName);
    if (macroName == null) {
      return false;
    }
    // Change the values of the effect parameters modulated by the macro as required too.
    var signalConnectionElementsWithMacroSource =
      ProgramXml.GetSignalConnectionElementsWithSource(macroName);
    foreach (var signalConnectionElement in signalConnectionElementsWithMacroSource) {
      var connectionsElement = ProgramXml.GetParentElement(signalConnectionElement);
      var effectElement = ProgramXml.GetParentElement(connectionsElement);
      var signalConnection = new SignalConnection(signalConnectionElement);
      ProgramXml.SetAttribute(
        effectElement, signalConnection.Destination,
        // If it's a toggle macro, Destination should be "Bypass".  
        signalConnection.Destination == "Bypass" ? 1 : 0);
    }
    return true;
  }

  public void ChangeMacroCcNo(int oldCcNo, int newCcNo) {
    Console.WriteLine($"Updating '{Path}'.");
    var oldSignalConnection = new SignalConnection { CcNo = oldCcNo };
    var newSignalConnection = new SignalConnection { CcNo = newCcNo };
    ProgramXml.ChangeSignalConnectionSource(oldSignalConnection, newSignalConnection);
  }

  public void ChangeReverbToZero() {
    foreach (var continuousMacro in ContinuousMacros.Where(
               continuousMacro =>
                 continuousMacro.ControlsReverb
                 && ChangeMacroValueToZero(continuousMacro.DisplayName))) {
      Console.WriteLine($"Changed {continuousMacro.DisplayName} to zero for '{Path}'.");
    }
  }

  private static void CheckForNonModWheelNonInfoPageMacro(
    SignalConnection signalConnection) {
    if (!signalConnection.IsForMacro
        // ReSharper disable once MergeIntoPattern
        && signalConnection.CcNo.HasValue && signalConnection.CcNo != 1) {
      throw new ApplicationException(
        $"MIDI CC {signalConnection.CcNo} is mapped to " +
        $"{signalConnection.Destination}, which is not an Info page macro.");
    }
  }

  /// <summary>
  ///   Modulation wheel (MIDI CC 1) is the only MIDI CC number expected not to control
  ///   a macro on the Info page. If there are others, there is a risk that they could
  ///   duplicate CC numbers we map to Info page macros.  So let's validate that there
  ///   are none.
  /// </summary>
  /// <remarks>
  ///   I've had to disable usage of this check in <see cref="Deserialise" /> because,
  ///   for unknown reason, it can report a false positive. In "Pulsar\Bass\Flipmode",
  ///   it reports CcNo 3 (Source="@MIDI CC 3").  Yet no such Source can be found in the
  ///   file. Strangely, the problem is not reproduced in a test category folder
  ///   containing only Flipmode and the template file.
  /// </remarks>
  [SuppressMessage("ReSharper", "CommentTypo")]
  [SuppressMessage("ReSharper", "UnusedMember.Local")]
  private void CheckForNonModWheelNonInfoPageMacros() {
    foreach (
      var signalConnection in Macros.SelectMany(macro =>
        macro.SignalConnections)) {
      CheckForNonModWheelNonInfoPageMacro(signalConnection);
    }
    foreach (
      var signalConnection in ScriptProcessors.SelectMany(scriptProcessor =>
        scriptProcessor.SignalConnections)) {
      CheckForNonModWheelNonInfoPageMacro(signalConnection);
    }
  }

  public void CountMacros() {
    if (Macros.Count > 10) {
      Console.WriteLine($"{Macros.Count} macros in '{Path}'.");
    }
  }

  private ProgramXml CreateProgramXml() {
    if (Category.SoundBankFolder.Name == "Organic Keys") {
      return new OrganicKeysProgramXml(Category, InfoPageCcsScriptProcessor!);
    }
    return Category.IsInfoPageLayoutInScript
      ? new ScriptProgramXml(Category, InfoPageCcsScriptProcessor!)
      : new ProgramXml(Category, InfoPageCcsScriptProcessor);
  }

  private void Deserialise() {
    using var reader = new StreamReader(Path);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    Name = root.Program.DisplayName;
    Macros = root.Program.Macros;
    ScriptProcessors = root.Program.ScriptProcessors;
    InfoPageCcsScriptProcessor = FindInfoPageCcsScriptProcessor();
    // Disabling this check for now, due to false positives.
    //CheckForNonModWheelNonInfoPageMacros();
  }

  /// <summary>
  ///   Finds the ScriptProcessor, if any, that is to contain the SignalConnections that
  ///   map the macros to MIDI CC numbers. If the ScriptProcessor is not found, each
  ///   macro's MIDI CC number must be defined in a SignalConnections owned by the
  ///   macro's ConstantModulation.
  /// </summary>
  private ScriptProcessor? FindInfoPageCcsScriptProcessor() {
    if (Category.SoundBankFolder.Name == "Factory"
        && !Category.IsInfoPageLayoutInScript) {
      // The macro MIDI CCs are defined for ScriptProcessor "EventProcessor9" if it
      // exists.
      return (
        from scriptProcessor in ScriptProcessors
        where scriptProcessor.Name == "EventProcessor9"
        select scriptProcessor).FirstOrDefault();
    }
    // Assume that the macro MIDI CCs are defined for the last ScriptProcessor, if any.
    return ScriptProcessors.Any() ? ScriptProcessors[^1] : null;
  }

  private int GetCcNo(Macro macro) {
    int result;
    if (macro.IsContinuous) {
      // Map continuous controller CC to continuous macro.
      NextContinuousCcNo = NextContinuousCcNo switch {
        39 => 41,
        49 => 51,
        59 => 61,
        _ => NextContinuousCcNo
      };
      result = NextContinuousCcNo switch {
        // Convert the fifth continuous controller's CC number to 11 to map to the touch
        // strip.
        35 => 11,
        // Convert MIDI CC 38, which does not work with macros on script-based Info
        // pages, to 28.
        38 => 28,
        _ => NextContinuousCcNo
      };
      NextContinuousCcNo++;
    } else {
      // Map button CC to toggle macro. 
      result = NextToggleCcNo;
      NextToggleCcNo++;
    }
    return result;
  }

  private List<Macro> GetContinuousMacros() {
    return (
      from macro in Macros
      where macro.IsContinuous
      select macro).ToList();
  }

  internal SortedSet<Macro> GetMacrosSortedByLocation(
    LocationOrder macroCcLocationOrder) {
    var result = new SortedSet<Macro>(
      macroCcLocationOrder == LocationOrder.TopToBottomLeftToRight
        ? new TopToBottomLeftToRightComparer()
        : new LeftToRightTopToBottomComparer());
    for (int i = 0; i < Macros.Count; i++) {
      var macro = Macros[i];
      // This validation is not reliable. In "Factory\Bells\Glowing 1.2", the macros with
      // ConstantModulation.Properties showValue="0" are shown on the Info page. 
      //macro.Properties.Validate();
      macro.Index = i;
      for (int j = 0; j < macro.SignalConnections.Count; j++) {
        var signalConnection = macro.SignalConnections[j];
        signalConnection.Index = j;
      }
      // In the Devinity sound bank, some macros do not appear on the Info page (only
      // the Mods page). For example Devinity/Bass/Comber Bass.
      // This is achieved by setting, in ConstantModulation.Properties, the X coordinates
      // of all those macros to 999, presumably off the right edge of the Info page, and
      // the Y coordinates to 353.
      // (Those ConstantModulation.Properties do not have the optional attribute
      // showValue="0".)
      // I don't know whether that is standard practice or just a trick in Devinity.
      // So, to prevent CC numbers from being given to macros that do not appear on the
      // Info page, omit all macros with duplicate locations from this set. Those macros
      // do not need CC numbers, and attempting to add duplicates to the set would throw
      // an exception in ConstantModulationLocationComparer.
      if (HasUniqueLocation(macro)) {
        result.Add(macro);
      }
    }
    return result;
  }

  private bool HasUniqueLocation(Macro macro) {
    return (
      from m in Macros
      where m.Properties.X == macro.Properties.X
            && m.Properties.Y == macro.Properties.Y
      select m).Count() == 1;
  }

  /// <summary>
  ///   Dual XML data load strategy:
  ///   To maximise forward compatibility with possible future changes to the program XML
  ///   data structure, we are deserialising only nodes we need, to the
  ///   ConstantModulations and ScriptProcessors lists. So we cannot serialise back to
  ///   file from those lists. Instead, the program XML file must be updated via
  ///   LINQ to XML in ProgramXml.
  /// </summary>
  public void Read() {
    Deserialise();
    ProgramXml = CreateProgramXml();
    ProgramXml.LoadFromFile(Path);
  }

  public void Save() {
    ProgramXml.SaveToFile(Path);
  }

  public void UpdateMacroCcs(LocationOrder macroCcLocationOrder) {
    Console.WriteLine($"Updating '{Path}'.");
    MacroCcLocationOrder = macroCcLocationOrder;
    if (Category.IsInfoPageLayoutInScript) {
      UpdateMacroCcsFromTemplateScriptProcessor();
      return;
    }
    // The category's Info page layout is specified in ConstantModulations.
    if (InfoPageCcsScriptProcessor == null) {
      // And the CCs are specified in the ConstantModulations. This is usual.
      UpdateMacroCcsInConstantModulations();
    } else {
      // But the CCs are specified for a script.
      UpdateMacroCcsInScriptProcessor();
    }
  }

  private void UpdateMacroCcsFromTemplateScriptProcessor() {
    InfoPageCcsScriptProcessor!.SignalConnections.Clear();
    foreach (var signalConnection in
             Category.TemplateScriptProcessor!.SignalConnections) {
      InfoPageCcsScriptProcessor.SignalConnections.Add(signalConnection);
    }
    ProgramXml.UpdateInfoPageCcsScriptProcessor();
  }

  /// <summary>
  ///   Where MIDI CC assignments to macros shown on the Info page are specified in
  ///   ConstantModulations,
  ///   updates the macro CCs so that the macros are successively assigned standard CCs
  ///   in the order of their locations on the Info page (top to bottom, left to right or
  ///   left to right, top to bottom, depending on <see cref="MacroCcLocationOrder" />).
  ///   There are different series of CCs for continuous and toggle macros.
  /// </summary>
  private void UpdateMacroCcsInConstantModulations() {
    // Most Factory programs list the ConstantModulation macro specifications in order
    // top to bottom, left to right. But a few, e.g. Factory/Keys/Days Of Old 1.4, do not.
    //
    var sortedByLocation =
      GetMacrosSortedByLocation(MacroCcLocationOrder);
    foreach (var macro in sortedByLocation) {
      //int ccNo = macro.GetCcNo(ref nextContinuousCcNo, ref nextToggleCcNo);
      // Retain unaltered any mapping to the modulation wheel (MIDI CC 1) or any other
      // MIDI CC mapping that's not on the Info page.
      // Example: Devinity/Bass/Comber Bass.
      var infoPageSignalConnections = (
        from sc in macro.SignalConnections
        where sc.IsForMacro
        select sc).ToList();
      // if (infoPageSignalConnections.Count !=
      //     macro.SignalConnections.Count) {
      //   Console.WriteLine($"Modulation wheel assignment found: {macro}");
      // }
      int ccNo = GetCcNo(macro);
      if (infoPageSignalConnections.Count == 0) { // Will be 0 or 1
        // The macro is not already mapped to a non-mod wheel CC number.
        var signalConnection = new SignalConnection {
          CcNo = ccNo
        };
        ProgramXml.AddMacroSignalConnection(
          signalConnection, macro.Index);
      } else {
        // The macro already has a SignalConnection mapping to a non-mod wheel CC number.
        // We need to conserve the SignalConnection tag, which might contain a custom
        // Ratio, and, with the exception below, just replace the CC number.
        var signalConnection = infoPageSignalConnections[0];
        signalConnection.CcNo = ccNo;
        // In Factory/Keys/Days Of Old 1.4, Macro 1, a switch macro, has Ratio -1 instead
        // of the usual 1. I don't know what the point of that is. But it prevents the
        // button controller mapped to the macro from working. To fix this, if a switch
        // macro has Ratio -1, update Ratio to 1. I cannot see any disadvantage in doing
        // that. 
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (!macro.IsContinuous && signalConnection.Ratio == -1) {
          signalConnection.Ratio = 1;
        }
        ProgramXml.UpdateMacroSignalConnection(
          macro, signalConnection);
      }
    }
  }

  /// <summary>
  ///   Where MIDI CC assignments to macros shown on the Info page are specified in a
  ///   ScriptProcessor,
  ///   updates the macro CCs so that the macros are successively assigned standard CCs
  ///   in the order of their locations on the Info page (top to bottom, left to right or
  ///   left to right, top to bottom, depending on <see cref="MacroCcLocationOrder" />).
  ///   There are different series of CCs for continuous and toggle macros.
  ///   Example: Factory/Keys/Smooth E-piano 2.1.
  /// </summary>
  private void UpdateMacroCcsInScriptProcessor() {
    var sortedByLocation =
      GetMacrosSortedByLocation(MacroCcLocationOrder);
    int macroNo = 0;
    // Any assignment of a macro the modulation wheel  or any other
    // MIDI CC mapping that's not on the Info page is expected to be
    // specified in a different ScriptProcessor. But let's check!
    bool infoPageCcsScriptProcessorHasModWheelSignalConnections = (
      from signalConnection in InfoPageCcsScriptProcessor!.SignalConnections
      where !signalConnection.IsForMacro
      select signalConnection).Any();
    if (infoPageCcsScriptProcessorHasModWheelSignalConnections) {
      // We've already validated against non-mod wheel CCs that don't control Info page
      // macros. So the reference to the mod wheel in this error message should be fine.
      throw new ApplicationException(
        "Modulation wheel assignment found in Info page CCs ScriptProcessor.");
    }
    InfoPageCcsScriptProcessor!.SignalConnections.Clear();
    foreach (var macro in sortedByLocation) {
      macroNo++;
      InfoPageCcsScriptProcessor.SignalConnections.Add(
        new SignalConnection {
          MacroNo = macroNo,
          CcNo = GetCcNo(macro)
        });
    }
    ProgramXml.UpdateInfoPageCcsScriptProcessor();
  }
}