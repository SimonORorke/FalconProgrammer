using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class ProgramConfig {
  //public const string ProgramExtension = ".uvip";
  private const string ProgramExtension = ".xml";
  private const string SynthName = "UVI Falcon";

  public ProgramConfig(
    string instrumentName, string templateProgramCategory, string templateProgramName) {
    InstrumentName = instrumentName;
    TemplateProgramCategory = templateProgramCategory;
    TemplateProgramName = templateProgramName;
  }

  private List<ConstantModulation> ConstantModulations { get; set; } = null!;
  [PublicAPI] public string InstrumentName { get; }
  [PublicAPI] public DirectoryInfo InstrumentProgramsFolder { get; private set; } = null!;
  public ScriptProcessor? MacroCcsScriptProcessor { get; private set; }

  /// <summary>
  ///   Name of ScriptProcessor, if any, that is to define the Info page macro CCs.
  /// </summary>
  /// <remarks>
  ///   In the Factory sound bank, sometimes there's an EventProcessor0 first, e.g. in
  ///   Factory/Keys/Smooth E-piano 2.1.
  ///   But Info page CC numbers don't go there.
  /// </remarks>
  public string MacroCcsScriptProcessorName { get; protected set; } = "EventProcessor9";

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")] 
  public string ProgramPath { get; private set; } = null!;
  
  protected ProgramXml ProgramXml { get; private set; } = null!;
  private List<ScriptProcessor> ScriptProcessors { get; set; } = null!;
  [PublicAPI] public string TemplateProgramCategory { get; }
  [PublicAPI] public string TemplateProgramName { get; }
  [PublicAPI] public string TemplateProgramPath { get; private set; } = null!;

  /// <summary>
  ///   Configures macro CCs for Falcon program presets.
  /// </summary>
  public virtual void ConfigureMacroCcs(string programCategory) {
    Initialise();
    var programFilesToEdit = GetProgramFilesToEdit(programCategory);
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
      ProgramXml.SaveToFile(programFileToEdit.FullName);
    }
  }

  protected virtual ProgramXml CreateProgramXml() {
    return new ProgramXml(TemplateProgramPath, this);
  }

  private void DeserialiseProgram() {
    using var reader = new StreamReader(ProgramPath);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    ConstantModulations = root.Program.ConstantModulations;
    ScriptProcessors = root.Program.ScriptProcessors;
    MacroCcsScriptProcessor = FindMacroCcsScriptProcessor();
  }

  private ScriptProcessor? FindMacroCcsScriptProcessor() {
    return (
      from scriptProcessor in ScriptProcessors
      where scriptProcessor.Name == MacroCcsScriptProcessorName
      select scriptProcessor).FirstOrDefault();
  }

  private SortedSet<ConstantModulation> GetConstantModulationsSortedByLocation() {
    var result = new SortedSet<ConstantModulation>(
      new ConstantModulationLocationComparer());
    for (int index = 0; index < ConstantModulations.Count; index++) {
      var constantModulation = ConstantModulations[index];
      constantModulation.Index = index;
      result.Add(constantModulation);
    }
    return result;
  }

  private DirectoryInfo GetInstrumentProgramsFolder() {
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
        synthSoftwareFolder.FullName, "Programs", InstrumentName));
    if (!result.Exists) {
      Console.Error.WriteLine($"Cannot find folder '{result.FullName}'.");
      Environment.Exit(1);
    }
    return result;
  }

  private IEnumerable<FileInfo> GetProgramFilesToEdit(string programCategory) {
    var folder = GetProgramsFolderToEdit(programCategory);
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

  private DirectoryInfo GetProgramsFolderToEdit(string programCategory) {
    var result = new DirectoryInfo(
      Path.Combine(InstrumentProgramsFolder.FullName, programCategory));
    if (!result.Exists) {
      Console.Error.WriteLine($"Cannot find folder '{result.FullName}'.");
      Environment.Exit(1);
    }
    return result;
  }

  private string GetTemplateProgramPath() {
    var templateProgramFile = new FileInfo(
      Path.Combine(InstrumentProgramsFolder.FullName,
        TemplateProgramCategory, TemplateProgramName + ProgramExtension));
    if (!templateProgramFile.Exists) {
      Console.Error.WriteLine($"Cannot find file '{templateProgramFile.FullName}'.");
      Environment.Exit(1);
    }
    return templateProgramFile.FullName;
  }

  protected virtual void Initialise() {
    InstrumentProgramsFolder = GetInstrumentProgramsFolder();
    TemplateProgramPath = GetTemplateProgramPath();
    ProgramXml = CreateProgramXml();
  }

  protected virtual void UpdateMacroCcs() {
    if (MacroCcsScriptProcessor != null) {
      Console.WriteLine($"Macro CCs ScriptProcessor in '{ProgramPath}'.");
      UpdateMacroCcsInScriptProcessor();
    } else {
      UpdateMacroCcsInConstantModulations();
    }
  }

  /// <summary>
  ///   Updates the macro CCs so that, top to bottom, left to right on the Info page,
  ///   the macros are successively assigned standard CCs in ascending order,
  ///   with different series of CCs for continuous and switch macros.
  /// </summary>
  private void UpdateMacroCcsInConstantModulations() {
    // Most Factory programs list the ConstantModulation macro specifications in order
    // top to bottom, left to right. But a few, e.g. Factory/Keys/Days Of Old 1.4, do not.
    // 
    // In the Devinity sound bank, the XML lists the
    // macros out of order and some macros do not appear on the info page.
    // (I have not yet been able to work out how to make a macro not appear on the Info
    // page or how tell from the XML whether it is or is not on the Info page.)
    // WILL THIS WORK?
    //
    var sortedByLocation = 
      GetConstantModulationsSortedByLocation();
    int nextContinuousCcNo = 31;
    int nextSwitchCcNo = 112;
    foreach (var constantModulation in sortedByLocation) {
      int ccNo = constantModulation.GetCcNo(ref nextContinuousCcNo, ref nextSwitchCcNo);
      if (constantModulation.SignalConnections.Count == 0) { // Will be 0 or 1
        // The macro is not already mapped to a CC number.
        var signalConnection = new SignalConnection { CcNo = ccNo };
        ProgramXml.AddConstantModulationSignalConnection(
          signalConnection, constantModulation.Index);
      } else {
        // The macro already has a Connections block mapping to a CC.
        // We need to conserve the SignalConnection tag, which might contain a custom
        // Ratio, and just replace the CC number.
        var signalConnection = constantModulation.SignalConnections[0];
        signalConnection.CcNo = ccNo;
        // In Factory/Keys/Days Of Old 1.4, Macro 1, a switch macro, has Ratio -1 instead
        // of the usual 1. I don't know what the point of that is. But it prevents the
        // button controller mapped to the macro from working. To fix this, if a switch
        // macro has Ratio -1, update Ratio to 1. I cannot see any disadvantage in doing
        // that. 
        if (!constantModulation.IsContinuous && signalConnection.Ratio == "-1") {
          signalConnection.Ratio = "1";
        }
        ProgramXml.UpdateConstantModulationSignalConnection(
          signalConnection, constantModulation.Index);
      }
    }
  }

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
    MacroCcsScriptProcessor!.SignalConnections.Clear();
    foreach (var constantModulation in sortedByLocation) {
      macroNo++;
      MacroCcsScriptProcessor.SignalConnections.Add(
        new SignalConnection {
          MacroNo = macroNo,
          CcNo = constantModulation.GetCcNo(ref nextContinuousCcNo, ref nextSwitchCcNo)
        });
    }
    ProgramXml.UpdateMacroCcsScriptProcessor();
  }
}