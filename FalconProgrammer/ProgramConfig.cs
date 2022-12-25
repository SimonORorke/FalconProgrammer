using FalconProgrammer.XmlDeserialised;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class ProgramConfig {
  private const int ModWheelReplacementCcNo = 34;
  public const string ProgramExtension = ".uvip";
  private const string SynthName = "UVI Falcon";
  private Category Category { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the order in which MIDI CC numbers are to be mapped to macros
  ///   relative to their locations on the Info page.
  /// </summary>
  [PublicAPI]
  public LocationOrder MacroCcLocationOrder { get; set; } =
    LocationOrder.TopToBottomLeftToRight;

  private FalconProgram Program { get; set; } = null!;
  private ProgramXml ProgramXml { get; set; } = null!;
  private DirectoryInfo SoundBankFolder { get; set; } = null!;
  private ConfigTask Task { get; set; }

  private void ConfigurePrograms(
    string soundBankName, string? categoryName = null) {
    SoundBankFolder = GetSoundBankFolder(soundBankName);
    if (categoryName != null) {
      ConfigureProgramsInCategory(categoryName);
    } else {
      foreach (var folder in SoundBankFolder.GetDirectories()) {
        if (!folder.Name.EndsWith(" ORIGINAL") && !folder.Name.EndsWith(" TEMPLATE")) {
          ConfigureProgramsInCategory(folder.Name);
        }
      }
    }
  }

  private void ConfigureProgramsInCategory(string categoryName) {
    Console.WriteLine("==========================");
    Console.WriteLine($"Category: {categoryName}");
    Category = new Category(categoryName, SoundBankFolder);
    Category.Initialise();
    if (Task == ConfigTask.ReplaceModWheelWithMacro &&
        Category.IsInfoPageLayoutInScript) {
      Console.WriteLine(
        $"Cannot replace mod wheel modulations for category '{categoryName}' " +
        "because the category's Info page layout is defined in a script.");
    }
    foreach (var programFileToEdit in Category.GetProgramFilesToEdit()) {
      ReadProgram(programFileToEdit);
      switch (Task) {
        case ConfigTask.ReplaceModWheelWithMacro:
          ReplaceModWheelWithMacroInProgram();
          break;
        case ConfigTask.UpdateMacroCcs:
          UpdateMacroCcsInProgram();
          break;
      }
      ProgramXml.SaveToFile(Program.Path);
    }
  }

  private ProgramXml CreateProgramXml() {
    if (SoundBankFolder.Name == "Organic Keys") {
      return new OrganicKeysProgramXml(
        Category.TemplateProgramPath, Program.InfoPageCcsScriptProcessor!);
    }
    return Category.IsInfoPageLayoutInScript
      ? new ScriptProgramXml(
        Category.TemplateProgramPath, Program.InfoPageCcsScriptProcessor!)
      : new ProgramXml(Category.TemplateProgramPath, Program.InfoPageCcsScriptProcessor);
  }

  public static DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var synthSoftwareFolder = new DirectoryInfo(
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "Music", "Software", SynthName));
    if (!synthSoftwareFolder.Exists) {
      throw new ApplicationException(
        $"Cannot find sound bank folder '{synthSoftwareFolder.FullName}'.");
    }
    var result = new DirectoryInfo(
      Path.Combine(
        synthSoftwareFolder.FullName, "Programs", soundBankName));
    if (!result.Exists) {
      throw new ApplicationException($"Cannot find folder '{result.FullName}'.");
    }
    return result;
  }

  /// <summary>
  ///   Dual XML data load strategy:
  ///   To maximise forward compatibility with possible future changes to the program XML
  ///   data structure, we are deserialising only nodes we need, to the
  ///   ConstantModulations and ScriptProcessors lists. So we cannot serialise back to
  ///   file from those lists. Instead, the program XML file must be updated via
  ///   LINQ to XML in ProgramXml.
  /// </summary>
  private void ReadProgram(FileSystemInfo programFileToEdit) {
    Program = new FalconProgram(programFileToEdit.FullName, Category);
    Program.Deserialise();
    ProgramXml = CreateProgramXml();
    ProgramXml.LoadFromFile(Program.Path);
  }

  /// <summary>
  ///   In each of the specified Falcon program presets where it is feasible, replaces
  ///   use of the modulation wheel with a Wheel macro that executes the same
  ///   modulations.
  /// </summary>
  [PublicAPI]
  public void ReplaceModWheelWithMacro(
    string soundBankName, string? categoryName = null) {
    Task = ConfigTask.ReplaceModWheelWithMacro;
    ConfigurePrograms(soundBankName, categoryName);
  }

  private void ReplaceModWheelWithMacroInProgram() {
    Console.WriteLine($"Checking '{Program.Path}'.");
    var continuousMacros = Program.GetContinuousMacros();
    string? existingWheelMacroDisplayName = (
      from continuousMacro in continuousMacros
      where continuousMacro.DisplayName.Contains("Wheel")
      select continuousMacro.DisplayName).FirstOrDefault();
    if (existingWheelMacroDisplayName != null) {
      Console.WriteLine(
        $"'{Program.Name}' already has a '{existingWheelMacroDisplayName}' macro.");
      return;
    }
    if (continuousMacros.Count > 3) {
      Console.WriteLine(
        $"'{Program.Name}' already has more than three continuous macros.");
      return;
    }
    bool hasReplacementCcNo = false;
    if (Program.InfoPageCcsScriptProcessor != null) {
      hasReplacementCcNo = (
        from signalConnection in Program.InfoPageCcsScriptProcessor.SignalConnections
        where signalConnection.CcNo == ModWheelReplacementCcNo
        select signalConnection).Any();
    } else {
      foreach (var continuousMacro in continuousMacros) {
        hasReplacementCcNo = (
          from signalConnection in continuousMacro.SignalConnections
          where signalConnection.CcNo == ModWheelReplacementCcNo
          select signalConnection).Any();
        if (hasReplacementCcNo) {
          break;
        }
      }
    }
    if (hasReplacementCcNo) {
      Console.WriteLine(
        $"'{Program.Name}' already has a macro mapped to mod wheel replacement " +
        $"MIDI CC {ModWheelReplacementCcNo}.");
      return;
    }
    if (!ProgramXml.FindModWheelModulations()) {
      Console.WriteLine($"'{Program.Name}' contains no mod wheel modulations.");
      return;
    }
    var locationForNewMacro = Program.GetLocationForNewMacro();
    if (locationForNewMacro == null) {
      Console.WriteLine(
        $"'{Program.Name}' " +
        "does not have room on its Info page's bottom row for a new macro.");
      return;
    }
    int newMacroNo = (
      from macro in Program.Macros select macro.MacroNo!.Value).Max() + 1;
    var newMacro = new ConstantModulation {
      MacroNo = newMacroNo,
      DisplayName = "Wheel",
      Bipolar = 0,
      IsContinuous = false,
      Value = 0,
      SignalConnections = new List<SignalConnection>() {
        new SignalConnection {
          CcNo = ModWheelReplacementCcNo
        }
      },
      Properties = new Properties {
        X = locationForNewMacro.Value.X,
        Y = locationForNewMacro.Value.Y
      }
    };
    ProgramXml.AddMacro(newMacro);
    ProgramXml.ChangeModWheelModulationSourcesToMacro(newMacroNo);
    Console.WriteLine(
      $"'{Program.Name}': Replaced mod wheel with macro.");
  }

  /// <summary>
  ///   Configures macro CCs for Falcon program presets.
  /// </summary>
  [PublicAPI]
  public void UpdateMacroCcs(
    string soundBankName, string? categoryName = null) {
    Task = ConfigTask.UpdateMacroCcs;
    ConfigurePrograms(soundBankName, categoryName);
  }

  private void UpdateMacroCcsInProgram() {
    Console.WriteLine($"Updating '{Program.Path}'.");
    if (Category.IsInfoPageLayoutInScript) {
      Program.InfoPageCcsScriptProcessor!.SignalConnections.Clear();
      foreach (var signalConnection in
               Category.TemplateScriptProcessor!.SignalConnections) {
        Program.InfoPageCcsScriptProcessor.SignalConnections.Add(signalConnection);
      }
      ProgramXml.UpdateInfoPageCcsScriptProcessor();
      return;
    }
    // The category's Info page layout is specified in ConstantModulations.
    if (Program.InfoPageCcsScriptProcessor != null) {
      Console.WriteLine($"Macro CCs ScriptProcessor in '{Program.Path}'.");
      UpdateMacroCcsInScriptProcessor();
    } else {
      UpdateMacroCcsInConstantModulations();
    }
  }

  /// <summary>
  ///   Where MIDI CC assignments to macros shown on the Info page are specified in
  ///   ConstantModulations,
  ///   updates the macro CCs so that the macros are successively assigned standard CCs
  ///   in the order of their locations on the Info page (top to bottom, left to right or
  ///   left to right, top to bottom, depending on <see cref="MacroCcLocationOrder" />).
  ///   There are different series of CCs for continuous and switch macros.
  /// </summary>
  private void UpdateMacroCcsInConstantModulations() {
    // Most Factory programs list the ConstantModulation macro specifications in order
    // top to bottom, left to right. But a few, e.g. Factory/Keys/Days Of Old 1.4, do not.
    //
    var sortedByLocation =
      Program.GetMacrosSortedByLocation(MacroCcLocationOrder);
    foreach (var constantModulation in sortedByLocation) {
      //int ccNo = constantModulation.GetCcNo(ref nextContinuousCcNo, ref nextToggleCcNo);
      // Retain unaltered any mapping to the modulation wheel (MIDI CC 1) or any other
      // MIDI CC mapping that's not on the Info page.
      // Example: Devinity/Bass/Comber Bass.
      var infoPageSignalConnections = (
        from sc in constantModulation.SignalConnections
        where sc.IsForInfoPageMacro
        select sc).ToList();
      // if (infoPageSignalConnections.Count !=
      //     constantModulation.SignalConnections.Count) {
      //   Console.WriteLine($"Modulation wheel assignment found: {constantModulation}");
      // }
      int ccNo = Program.GetCcNo(constantModulation);
      if (infoPageSignalConnections.Count == 0) { // Will be 0 or 1
        // The macro is not already mapped to a non-mod wheel CC number.
        var signalConnection = new SignalConnection {
          CcNo = ccNo
        };
        ProgramXml.AddConstantModulationSignalConnection(
          signalConnection, constantModulation.Index);
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
        if (!constantModulation.IsContinuous && signalConnection.Ratio == -1) {
          signalConnection.Ratio = 1;
        }
        ProgramXml.UpdateConstantModulationSignalConnection(
          constantModulation, signalConnection);
      }
    }
  }

  /// <summary>
  ///   Where MIDI CC assignments to macros shown on the Info page are specified in a
  ///   ScriptProcessor,
  ///   updates the macro CCs so that the macros are successively assigned standard CCs
  ///   in the order of their locations on the Info page (top to bottom, left to right or
  ///   left to right, top to bottom, depending on <see cref="MacroCcLocationOrder" />).
  ///   There are different series of CCs for continuous and switch macros.
  /// </summary>
  private void UpdateMacroCcsInScriptProcessor() {
    // Assign button CCs to switch macros. 
    // Example: Factory/Keys/Smooth E-piano 2.1.
    //
    var sortedByLocation =
      Program.GetMacrosSortedByLocation(MacroCcLocationOrder);
    int macroNo = 0;
    // Any assignment of a macro the modulation wheel  or any other
    // MIDI CC mapping that's not on the Info page is expected to be
    // specified in a different ScriptProcessor. But let's check!
    bool infoPageCcsScriptProcessorHasModWheelSignalConnections = (
      from signalConnection in Program.InfoPageCcsScriptProcessor!.SignalConnections
      where !signalConnection.IsForInfoPageMacro
      select signalConnection).Any();
    if (infoPageCcsScriptProcessorHasModWheelSignalConnections) {
      // We've already validated against non-mod wheel CCs that don't control Info page
      // macros. So the reference to the mod wheel in this error message should be fine.
      throw new ApplicationException(
        "Modulation wheel assignment found in Info page CCs ScriptProcessor.");
    }
    Program.InfoPageCcsScriptProcessor!.SignalConnections.Clear();
    foreach (var constantModulation in sortedByLocation) {
      macroNo++;
      Program.InfoPageCcsScriptProcessor.SignalConnections.Add(
        new SignalConnection {
          MacroNo = macroNo,
          CcNo = Program.GetCcNo(constantModulation)
        });
    }
    ProgramXml.UpdateInfoPageCcsScriptProcessor();
  }

  private enum ConfigTask {
    ReplaceModWheelWithMacro,
    UpdateMacroCcs
  }
}