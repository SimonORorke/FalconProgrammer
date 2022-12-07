using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class ProgramConfig {
  private const string ProgramExtension = ".uvip";
  // private const string ProgramExtension = ".xml";
  private const string SynthName = "UVI Falcon";

  public ProgramConfig(
    string templateSoundBankName = "Factory",
    string templateCategoryName = "Keys WITH TEMPLATE",
    string templateProgramName = "DX Mania") {
    TemplateSoundBankName = templateSoundBankName;
    TemplateCategoryName = templateCategoryName;
    TemplateProgramName = templateProgramName;
  }

  private List<ConstantModulation> ConstantModulations { get; set; } = null!;
  protected ScriptProcessor? InfoPageCcsScriptProcessor { get; private set; }

  /// <summary>
  ///   Name of ScriptProcessor, if any, that is to define the Info page macro CCs.
  /// </summary>
  /// <remarks>
  ///   In the Factory sound bank, sometimes there's an EventProcessor0 first, e.g. in
  ///   Factory/Keys/Smooth E-piano 2.1.
  ///   But Info page CC numbers don't go there.
  /// </remarks>
  protected string InfoPageCcsScriptProcessorName { get; set; } = "EventProcessor9";

  private DirectoryInfo SoundBankFolder { get; set; } = null!;
  [PublicAPI] public string TemplateSoundBankName { get; }

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public string ProgramPath { get; private set; } = null!;

  protected ProgramXml ProgramXml { get; private set; } = null!;
  private List<ScriptProcessor> ScriptProcessors { get; set; } = null!;
  [PublicAPI] public string TemplateCategoryName { get; }
  [PublicAPI] public string TemplateProgramName { get; }
  [PublicAPI] public string TemplateProgramPath { get; private set; } = null!;

  private void CheckForNonModWheelNonInfoPageSignalConnection(
    SignalConnection signalConnection) {
    if (!signalConnection.IsForInfoPageMacro && signalConnection.CcNo != 1) {
      throw new ApplicationException(
        $"Error in '{ProgramPath}':\r\n:" +
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
  private void CheckForNonModWheelNonInfoPageSignalConnections() {
    foreach (
      var signalConnection in ConstantModulations.SelectMany(constantModulation =>
        constantModulation.SignalConnections)) {
      CheckForNonModWheelNonInfoPageSignalConnection(signalConnection);
    }
    foreach (
      var signalConnection in ScriptProcessors.SelectMany(scriptProcessor =>
        scriptProcessor.SignalConnections)) {
      CheckForNonModWheelNonInfoPageSignalConnection(signalConnection);
    }
  }

  /// <summary>
  ///   Configures macro CCs for Falcon program presets.
  /// </summary>
  public virtual void ConfigureMacroCcs(
    string soundBankName, string categoryName) {
    Initialise();
    SoundBankFolder = GetSoundBankFolder(soundBankName);
    var programFilesToEdit = GetProgramFilesToEdit(categoryName);
    foreach (var programFileToEdit in programFilesToEdit) {
      ProgramPath = programFileToEdit.FullName;
      Console.WriteLine($"Updating '{ProgramPath}'.");
      // Dual XML data load strategy:
      // To maximise forward compatibility with possible future changes to the program XML
      // data structure, we are deserialising only nodes we need, to the
      // ConstantModulations and ScriptProcessors lists. So we cannot serialise back to
      // file from those lists. Instead, the program XML file must be updated via
      // LINQ to XML in ProgramXml. 
      DeserialiseProgram();
      ProgramXml.LoadFromFile(ProgramPath);
      UpdateMacroCcs();
      ProgramXml.SaveToFile(ProgramPath);
    }
  }

  protected virtual ProgramXml CreateProgramXml() {
    return new ProgramXml(TemplateProgramPath, InfoPageCcsScriptProcessor);
  }

  private void DeserialiseProgram() {
    using var reader = new StreamReader(ProgramPath);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    ConstantModulations = root.Program.ConstantModulations;
    ScriptProcessors = root.Program.ScriptProcessors;
    InfoPageCcsScriptProcessor = FindInfoPageCcsScriptProcessor();
    CheckForNonModWheelNonInfoPageSignalConnections();
  }

  private ScriptProcessor? FindInfoPageCcsScriptProcessor() {
    return (
      from scriptProcessor in ScriptProcessors
      where scriptProcessor.Name == InfoPageCcsScriptProcessorName
      select scriptProcessor).FirstOrDefault();
  }

  private SortedSet<ConstantModulation> GetConstantModulationsSortedByLocation() {
    var result = new SortedSet<ConstantModulation>(
      new ConstantModulationLocationComparer());
    for (int i = 0; i < ConstantModulations.Count; i++) {
      var constantModulation = ConstantModulations[i];
      constantModulation.Index = i;
      for (int j = 0; j < constantModulation.SignalConnections.Count; j++) {
        var signalConnection = constantModulation.SignalConnections[j];
        signalConnection.Index = j;
      }
      // In the Devinity sound bank, some macros do not appear on the Info page (only
      // the Mods page). For example Devinity/Bass/Comber Bass.
      // This is achieved by setting the X coordinates of all those macros to 999,
      // presumably off the right edge of the Info page, and the Y coordinates to 353. I
      // don't know whether that is standard practice or just a trick in Devinity.
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

  private IEnumerable<FileInfo> GetProgramFilesToEdit(string categoryName) {
    var folder = GetProgramsFolderToEdit(categoryName);
    var programFiles = folder.GetFiles("*" + ProgramExtension);
    var result = (
      from programFile in programFiles
      where programFile.FullName != TemplateProgramPath
      select programFile).ToList();
    if (result.Count == 0) {
      Console.Error.WriteLine(
        $"There are no program files to edit in folder '{folder.FullName}'.");
      Environment.Exit(1);
    }
    return result;
  }

  private DirectoryInfo GetProgramsFolderToEdit(string categoryName) {
    var result = new DirectoryInfo(
      Path.Combine(SoundBankFolder.FullName, categoryName));
    if (!result.Exists) {
      Console.Error.WriteLine($"Cannot find folder '{result.FullName}'.");
      Environment.Exit(1);
    }
    return result;
  }

  private static DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var synthSoftwareFolder = new DirectoryInfo(
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "Music", "Software", SynthName));
    if (!synthSoftwareFolder.Exists) {
      Console.Error.WriteLine(
        $"Cannot find folder '{synthSoftwareFolder.FullName}'.");
      Environment.Exit(1);
    }
    var result = new DirectoryInfo(
      Path.Combine(
        synthSoftwareFolder.FullName, "Programs", soundBankName));
    if (!result.Exists) {
      Console.Error.WriteLine($"Cannot find folder '{result.FullName}'.");
      Environment.Exit(1);
    }
    return result;
  }

  private string GetTemplateProgramPath() {
    var templateProgramFile = new FileInfo(
      Path.Combine(GetSoundBankFolder(TemplateSoundBankName).FullName,
        TemplateCategoryName, TemplateProgramName + ProgramExtension));
    if (!templateProgramFile.Exists) {
      Console.Error.WriteLine($"Cannot find file '{templateProgramFile.FullName}'.");
      Environment.Exit(1);
    }
    return templateProgramFile.FullName;
  }

  private bool HasUniqueLocation(ConstantModulation constantModulation) {
    return (
      from cm in ConstantModulations
      where cm.Properties.X == constantModulation.Properties.X
            && cm.Properties.Y == constantModulation.Properties.Y
      select cm).Count() == 1;
  }

  protected virtual void Initialise() {
    TemplateProgramPath = GetTemplateProgramPath();
    ProgramXml = CreateProgramXml();
  }

  protected virtual void UpdateMacroCcs() {
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
  ///   updates the macro CCs so that, top to bottom, left to right on the Info page,
  ///   the macros are successively assigned standard CCs in ascending order,
  ///   with different series of CCs for continuous and switch macros.
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
  ///   updates the macro CCs so that, top to bottom, left to right on the Info page,
  ///   the macros are successively assigned standard CCs in ascending order,
  ///   with different series of CCs for continuous and switch macros.
  /// </summary>
  private void UpdateMacroCcsInScriptProcessor() {
    // In Factory/Keys, the only Factory program category I've looked at so far, all
    // programs with macro CCs specified in the ScriptProcessor are listed in the
    // ConstantModulations in top to bottom order. But ConstantModulation.Properties
    // does specify the location. I don't (yet) know of any Falcon programs where this is
    // not the case. But, just in case,
    // assign the CC numbers top to bottom, left to right.
    //
    // In some sound banks, such as Organic Keys, ConstantModulations specify only
    // modulation wheel assignment, not macros. In those cases, the custom CC number
    // assignment needs to be modelled on a template program in ScriptConfig. 
    //
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
        $"Error in '{ProgramPath}'.\r\n:" +
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