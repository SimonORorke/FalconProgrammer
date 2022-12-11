using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class ProgramConfig {
  private const string ProgramExtension = ".uvip";
  private const string SynthName = "UVI Falcon";
  private DirectoryInfo CategoryFolder { get; set; } = null!;
  private List<ConstantModulation> ConstantModulations { get; set; } = null!;
  private ScriptProcessor? InfoPageCcsScriptProcessor { get; set; }

  /// <summary>
  ///   Name of ScriptProcessor, if any, that is to define the Info page macro CCs.
  /// </summary>
  /// <remarks>
  ///   In the Factory sound bank, sometimes there's an EventProcessor0 first, e.g. in
  ///   Factory/Keys/Smooth E-piano 2.1.
  ///   But Info page CC numbers don't go there.
  /// </remarks>
  private string InfoPageCcsScriptProcessorName { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the order in which MIDI CC numbers are to be mapped to macros
  ///   relative to their locations on the Info page.
  /// </summary>
  [PublicAPI]
  public LocationOrder MacroCcLocationOrder { get; set; } =
    LocationOrder.TopToBottomLeftToRight;

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public string ProgramPath { get; private set; } = null!;

  private ProgramXml ProgramXml { get; set; } = null!;
  private List<ScriptProcessor> ScriptProcessors { get; set; } = null!;
  private DirectoryInfo SoundBankFolder { get; set; } = null!;
  [PublicAPI] public string TemplateCategoryName { get; private set; } = "Keys";
  [PublicAPI] public string TemplateProgramName { get; private set; } = "DX Mania";
  [PublicAPI] public string TemplateProgramPath { get; private set; } = null!;
  private ScriptProcessor? TemplateScriptProcessor { get; set; }
  private string? TemplateScriptProcessorName { get; set; }
  [PublicAPI] public string TemplateSoundBankName { get; private set; } = "Factory";

  private static void CheckForNonModWheelNonInfoPageMacro(
    SignalConnection signalConnection) {
    if (!signalConnection.IsForInfoPageMacro
        // ReSharper disable once MergeIntoPattern
        && signalConnection.CcNo.HasValue && signalConnection.CcNo != 1) {
      throw new ApplicationException(
        $"MIDI CC {signalConnection.CcNo} is mapped to " +
        $"{signalConnection.Destination}, which is not a Info page macro.");
    }
  }

  /// <summary>
  ///   Modulation wheel (MIDI CC 1) is the only MIDI CC number expected not to control
  ///   a macro on the Info page. If there are others, there is a risk that they could
  ///   duplicate CC numbers we map to Info page macros.  So let's validate that there
  ///   are none.
  /// </summary>
  private void CheckForNonModWheelNonInfoPageMacros() {
    foreach (
      var signalConnection in ConstantModulations.SelectMany(constantModulation =>
        constantModulation.SignalConnections)) {
      CheckForNonModWheelNonInfoPageMacro(signalConnection);
    }
    foreach (
      var signalConnection in ScriptProcessors.SelectMany(scriptProcessor =>
        scriptProcessor.SignalConnections)) {
      CheckForNonModWheelNonInfoPageMacro(signalConnection);
    }
  }

  /// <summary>
  ///   Configures macro CCs for Falcon program presets.
  /// </summary>
  [PublicAPI]
  public void ConfigureMacroCcs(
    string soundBankName, string? categoryName = null) {
    SoundBankFolder = GetSoundBankFolder(soundBankName);
    if (categoryName != null) {
      ConfigureMacroCcsForCategory(categoryName);
    } else {
      foreach (var folder in SoundBankFolder.GetDirectories()) {
        if (!folder.Name.EndsWith(" ORIGINAL") && !folder.Name.EndsWith(" TEMPLATE")) {
          ConfigureMacroCcsForCategory(folder.Name);
        }
      }
    }
  }

  private void ConfigureMacroCcsForCategory(string categoryName) {
    Console.WriteLine("==========================");
    Console.WriteLine($"Category: {categoryName}");
    TemplateSoundBankName = IsCategoryInfoPageLayoutInScriptProcessor(categoryName) 
      ? SoundBankFolder.Name 
      : "Factory";
    TemplateCategoryName = GetTemplateCategoryName(categoryName);
    TemplateProgramName = GetTemplateProgramName(categoryName);
    TemplateProgramPath = GetTemplateProgramPath();
    var programFilesToEdit = GetCategoryProgramFilesToEdit(categoryName);
    TemplateScriptProcessorName = GetTemplateScriptProcessorName();
    if (IsCategoryInfoPageLayoutInScriptProcessor(categoryName)) {
      DeserialiseTemplateProgram();
    } else {
      TemplateScriptProcessor = null;
    }
    InfoPageCcsScriptProcessorName = GetInfoPageCcsScriptProcessorName();
    foreach (var programFileToEdit in programFilesToEdit) {
      ConfigureMacroCcsForProgram(programFileToEdit);
    }
  }

  private void ConfigureMacroCcsForProgram(FileSystemInfo programFileToEdit) {
    ProgramPath = programFileToEdit.FullName;
    Console.WriteLine($"Updating '{ProgramPath}'.");
    // Dual XML data load strategy:
    // To maximise forward compatibility with possible future changes to the program XML
    // data structure, we are deserialising only nodes we need, to the
    // ConstantModulations and ScriptProcessors lists. So we cannot serialise back to
    // file from those lists. Instead, the program XML file must be updated via
    // LINQ to XML in ProgramXml. 
    DeserialiseProgram();
    ProgramXml = CreateProgramXml();
    ProgramXml.LoadFromFile(ProgramPath);
    UpdateMacroCcs();
    ProgramXml.SaveToFile(ProgramPath);
  }

  private ProgramXml CreateProgramXml() {
    if (SoundBankFolder.Name == "Organic Keys") {
      return new OrganicKeysProgramXml(TemplateProgramPath, InfoPageCcsScriptProcessor!);
    }
    return IsCategoryInfoPageLayoutInScriptProcessor(CategoryFolder.Name) 
      ? new ScriptProgramXml(TemplateProgramPath, InfoPageCcsScriptProcessor!) 
      : new ProgramXml(TemplateProgramPath, InfoPageCcsScriptProcessor);
  }

  private void DeserialiseProgram() {
    using var reader = new StreamReader(ProgramPath);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    ConstantModulations = root.Program.ConstantModulations;
    ScriptProcessors = root.Program.ScriptProcessors;
    InfoPageCcsScriptProcessor = FindInfoPageCcsScriptProcessor();
    CheckForNonModWheelNonInfoPageMacros();
  }

  private void DeserialiseTemplateProgram() {
    using var reader = new StreamReader(TemplateProgramPath);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    TemplateScriptProcessor =
      (from scriptProcessor in root.Program.ScriptProcessors
        where scriptProcessor.Name == TemplateScriptProcessorName
        select scriptProcessor).FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find {TemplateScriptProcessorName} in file '{TemplateProgramPath}'.");
  }

  private ScriptProcessor? FindInfoPageCcsScriptProcessor() {
    if (SoundBankFolder.Name == "Factory" &&
        CategoryFolder.Name == "Organic Texture 2.8") {
      // The Info page layout ScriptProcessor name is not consistent across all programs
      // in this category. E.g. for "BEL SoToy" it's "EventProcessor1", while for
      // programs alphabetically prior to that one it's "EventProcessor0". But there
      // should only be one ScriptProcessor in each program in the category.
      return (
        from scriptProcessor in ScriptProcessors
        select scriptProcessor).FirstOrDefault();
    }
    return (
      from scriptProcessor in ScriptProcessors
      where scriptProcessor.Name == InfoPageCcsScriptProcessorName
      select scriptProcessor).FirstOrDefault();
  }

  private SortedSet<ConstantModulation> GetConstantModulationsSortedByLocation() {
    var result = new SortedSet<ConstantModulation>(
      MacroCcLocationOrder == LocationOrder.TopToBottomLeftToRight
        ? new TopToBottomLeftToRightComparer()
        : new LeftToRightTopToBottomComparer());
    for (int i = 0; i < ConstantModulations.Count; i++) {
      var constantModulation = ConstantModulations[i];
      // This validation is not reliable. In "Factory\Bells\Glowing 1.2", the macros with
      // ConstantModulation.Properties showValue="0" are shown on the Info page. 
      //constantModulation.Properties.Validate();
      constantModulation.Index = i;
      for (int j = 0; j < constantModulation.SignalConnections.Count; j++) {
        var signalConnection = constantModulation.SignalConnections[j];
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
      if (HasUniqueLocation(constantModulation)) {
        result.Add(constantModulation);
      }
    }
    return result;
  }

  private string GetInfoPageCcsScriptProcessorName() {
    if (IsCategoryInfoPageLayoutInScriptProcessor(CategoryFolder.Name) 
        && (SoundBankFolder.Name != "Voklm" 
            || CategoryFolder.Name != "Vox Instruments")) {
      return TemplateScriptProcessorName!;
    }
    // Info page layout is defined in ConstantModulations
    // or category is "Voklm/Vox Instruments".
    return "EventProcessor9"; 
  }

  private IEnumerable<FileInfo> GetCategoryProgramFilesToEdit(string categoryName) {
    CategoryFolder = GetCategoryFolder(categoryName);
    var programFiles = CategoryFolder.GetFiles("*" + ProgramExtension);
    var result = (
      from programFile in programFiles
      where programFile.FullName != TemplateProgramPath
      select programFile).ToList();
    if (result.Count == 0) {
      throw new ApplicationException(
        $"There are no program files to edit in folder '{CategoryFolder.FullName}'.");
    }
    return result;
  }

  private DirectoryInfo GetCategoryFolder(string categoryName) {
    var result = new DirectoryInfo(
      Path.Combine(SoundBankFolder.FullName, categoryName));
    if (!result.Exists) {
      throw new ApplicationException($"Cannot find folder '{result.FullName}'.");
    }
    return result;
  }

  private static DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var synthSoftwareFolder = new DirectoryInfo(
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "Music", "Software", SynthName));
    if (!synthSoftwareFolder.Exists) {
      throw new ApplicationException(
        $"Cannot find folder '{synthSoftwareFolder.FullName}'.");
    }
    var result = new DirectoryInfo(
      Path.Combine(
        synthSoftwareFolder.FullName, "Programs", soundBankName));
    if (!result.Exists) {
      throw new ApplicationException($"Cannot find folder '{result.FullName}'.");
    }
    return result;
  }

  private string GetTemplateCategoryName(string categoryName) {
    if (TemplateSoundBankName == "Factory" && categoryName == "Organic Texture 2.8") {
      return categoryName;
    }
    return TemplateSoundBankName switch {
      "Hypnotic Drive" => "Leads",
      "Organic Keys" => "Acoustic Mood",
      "Voklm" => "Synth Choirs",
      _ => "Keys"
    };
  }

  private string GetTemplateProgramName(string categoryName) {
    if (TemplateSoundBankName == "Factory" && categoryName == "Organic Texture 2.8") {
      return "ARP Breather";
    }
    return TemplateSoundBankName switch {
      "Hypnotic Drive" => "Lead - Acid Gravel",
      "Organic Keys" => "A Rhapsody",
      "Voklm" => "Breath Five",
      _ => "DX Mania"
    };
  }

  private string GetTemplateProgramPath() {
    var templateProgramFile = new FileInfo(
      Path.Combine(GetSoundBankFolder(TemplateSoundBankName).FullName,
        TemplateCategoryName, TemplateProgramName + ProgramExtension));
    if (!templateProgramFile.Exists) {
      throw new ApplicationException(
        $"Cannot find template file '{templateProgramFile.FullName}'.");
    }
    return templateProgramFile.FullName;
  }

  private string? GetTemplateScriptProcessorName() {
    if (IsCategoryInfoPageLayoutInScriptProcessor(CategoryFolder.Name)) {
      return TemplateSoundBankName switch {
        "Hypnotic Drive" => "EventProcessor99",
        _ => "EventProcessor0"
      };
    }
    return null;
  }

  private bool HasUniqueLocation(ConstantModulation constantModulation) {
    return (
      from cm in ConstantModulations
      where cm.Properties.X == constantModulation.Properties.X
            && cm.Properties.Y == constantModulation.Properties.Y
      select cm).Count() == 1;
  }

  /// <summary>
  ///   In some sound banks, such as Organic Keys, ConstantModulations do not specify
  ///   Info page macros, only modulation wheel assignment. In others, such as
  ///   Hypnotic Drive, ConstantModulation.Properties include the optional attribute
  ///   showValue="0", indicating that the coordinates specified in the Properties will
  ///   not actually be used to determine the locations of macros on the Info page.
  ///   <para>
  ///     In these cases, the Info page layout is specified in a script.
  ///     SignalConnections mapping MIDI CC numbers to macros must be added to that
  ///     script's ScriptProcessor. The SignalConnections are copied from a template
  ///     program file specific to the Info page layout.
  ///   </para>
  ///   <para>
  ///     There is generally one template program file per sound bank, supporting a
  ///     common Info page layout defined in a single script for the whole sound bank.
  ///     However, in the Factory sound bank, "Organic Texture 2.8" is the only category
  ///     for which a script defines the Info page layout. So in that case, there is a
  ///     category-specific template program file.
  ///   </para>
  /// </summary>
  private bool IsCategoryInfoPageLayoutInScriptProcessor(string categoryName) {
    switch (SoundBankFolder.Name) {
      case "Hypnotic Drive":
      case "Organic Keys":
      case "Voklm":
        return true;
      case "Factory":
        return categoryName == "Organic Texture 2.8";
      default:
        return false;
    }
  }

  private void UpdateMacroCcs() {
    if (IsCategoryInfoPageLayoutInScriptProcessor(CategoryFolder.Name)) {
      InfoPageCcsScriptProcessor!.SignalConnections.Clear();
      foreach (var signalConnection in TemplateScriptProcessor!.SignalConnections) {
        InfoPageCcsScriptProcessor.SignalConnections.Add(signalConnection);
      }
      ProgramXml.UpdateInfoPageCcsScriptProcessor();
      return;
    }
    // The category's Info page layout is specified in ConstantModulations.
    if (InfoPageCcsScriptProcessor != null) {
      Console.WriteLine($"Macro CCs ScriptProcessor in '{ProgramPath}'.");
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
      GetConstantModulationsSortedByLocation();
    int nextContinuousCcNo = 31;
    int nextSwitchCcNo = 112;
    foreach (var constantModulation in sortedByLocation) {
      int ccNo = constantModulation.GetCcNo(ref nextContinuousCcNo, ref nextSwitchCcNo);
      // Retain unaltered any mapping to the modulation wheel (MIDI CC 1) or any other
      // MIDI CC mapping that's not on the Info page.
      // Example: Devinity/Bass/Comber Bass.
      var infoPageSignalConnections = (
        from sc in constantModulation.SignalConnections
        where sc.IsForInfoPageMacro
        select sc).ToList();
      if (infoPageSignalConnections.Count !=
          constantModulation.SignalConnections.Count) {
        Console.WriteLine($"Modulation wheel assignment found: {constantModulation}");
      }
      if (infoPageSignalConnections.Count == 0) { // Will be 0 or 1
        // The macro is not already mapped to a non-mod wheel CC number.
        var signalConnection = new SignalConnection { CcNo = ccNo };
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
      GetConstantModulationsSortedByLocation();
    int macroNo = 0;
    int nextContinuousCcNo = 31;
    int nextSwitchCcNo = 112;
    // Any assignment of a macro the modulation wheel  or any other
    // MIDI CC mapping that's not on the Info page is expected to be
    // specified in a different ScriptProcessor. But let's check!
    bool infoPageCcsScriptProcessorHasModWheelSignalConnections = (
      from signalConnection in InfoPageCcsScriptProcessor!.SignalConnections
      where !signalConnection.IsForInfoPageMacro
      select signalConnection).Any();
    if (infoPageCcsScriptProcessorHasModWheelSignalConnections) {
      // We've already validated against non-mod wheel CCs that don't control Info page
      // macros. So the reference to the mod wheel in this error message should be fine.
      throw new ApplicationException(
        "Modulation wheel assignment found in Info page CCs ScriptProcessor.");
    }
    InfoPageCcsScriptProcessor!.SignalConnections.Clear();
    foreach (var constantModulation in sortedByLocation) {
      macroNo++;
      InfoPageCcsScriptProcessor.SignalConnections.Add(
        new SignalConnection {
          MacroNo = macroNo,
          CcNo = constantModulation.GetCcNo(ref nextContinuousCcNo, ref nextSwitchCcNo)
        });
    }
    ProgramXml.UpdateInfoPageCcsScriptProcessor();
  }
}