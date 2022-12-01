using System.Text.RegularExpressions;
using System.Xml.Serialization;
using FalconProgrammer.XmlModels;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class ProgramConfig {
  private const string ConnectionsStartTag = "<Connections>";
  private const string ConstantModulationStartTag = "<ConstantModulation ";
  private const string ControlSignalSourcesEndTag = "</ControlSignalSources>";
  /// <summary>
  /// Sometimes there's an EventProcessor0 first, e.g. in
  /// Factory/Keys/Smooth E-piano 2.1.
  /// But Info page CC numbers don't go there.
  /// </summary>
  public const string MacroCcsScriptProcessorName = "EventProcessor9";
  
  public ProgramConfig(
    string instrumentName, string templateProgramCategory, string templateProgramName) {
    InstrumentName = instrumentName;
    TemplateProgramCategory = templateProgramCategory;
    TemplateProgramName = templateProgramName;
  }

  private string ConnectionsEndLine{ get; set; } = null!;
  private string ConnectionsStartLine{ get; set; } = null!;
  private List<ConstantModulation> ConstantModulations { get; set; } = null!;
  [PublicAPI] public string InstrumentName { get; }
  [PublicAPI] public DirectoryInfo InstrumentProgramsFolder { get; private set; } = null!;
  private ScriptProcessor? MacroCcsScriptProcessor { get; set; }
  [PublicAPI] public string MainCcTemplate { get; private set; } = null!;
  private ProgramXml ProgramXml { get; set; } = null!;
  private List<ScriptProcessor> ScriptProcessors { get; set; } = null!;
  [PublicAPI] public string SignalProcessorCcTemplate { get; private set; } = null!;
  [PublicAPI] public string TemplateProgramCategory { get; }
  [PublicAPI] public string TemplateProgramName { get; }
  [PublicAPI] public string TemplateProgramPath { get; private set; } = null!;

  /// <summary>
  ///   Configures macro CCs for Falcon program presets.
  /// </summary>
  public virtual void ConfigureCcs(string programCategory) {
    Initialise();
    var programFilesToEdit = GetProgramFilesToEdit(programCategory);
    foreach (var programFileToEdit in programFilesToEdit) {
      Console.WriteLine($"Updating '{programFileToEdit.FullName}'.");
      UpdateCCs(programFileToEdit.FullName);
    }
  }

  private void DeserialiseProgramXml(string programPath) {
    using var reader = new StreamReader(programPath);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    ConstantModulations = root.Program.ConstantModulations;
    ScriptProcessors = root.Program.ScriptProcessors;
  }

  private ScriptProcessor? FindMacroCcsScriptProcessor() {
    return (
      from scriptProcessor in ScriptProcessors
      where scriptProcessor.Name == MacroCcsScriptProcessorName
      select scriptProcessor).FirstOrDefault();
  }

  private DirectoryInfo GetInstrumentProgramsFolder() {
    var synthSoftwareFolder = new DirectoryInfo(
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "Music", "Software", Utility.SynthName));
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

  protected virtual string GetMainCcTemplate() {
    using var reader = new StreamReader(TemplateProgramPath);
    string line = string.Empty;
    while (!line.Contains(ConnectionsStartTag) && !reader.EndOfStream) {
      line = reader.ReadLine()!;
    }
    if (!line.Contains(ConnectionsStartTag)) {
      Console.Error.WriteLine(
        $"Cannot find '{ConnectionsStartTag}' in file '{TemplateProgramPath}'.");
    }
    // Start of first macro's Connections block
    using var writer = new StringWriter();
    writer.WriteLine(line);
    line = reader.ReadLine()!; // SignalConnection tag line
    // If the SignalConnection contains a non-default (< 1) Ratio,
    // change the Ratio to the default, 1.
    // E.g. replaces 
    //    Ratio="0.36663184"
    // with
    //    Ratio="1"
    const string pattern = "Ratio=\"0\\.\\d+\"";
    const string replacement = "Ratio=\"1\"";
    string signalConnectionLine = Regex.Replace(line, pattern, replacement);
    writer.WriteLine(signalConnectionLine);
    line = reader.ReadLine()!; // Connections end tag
    writer.WriteLine(line);
    // We have written all three lines of the first macro's Connections block,
    // which contains the MIDI CC.
    return writer.ToString();
  }

  private IEnumerable<FileInfo> GetProgramFilesToEdit(string programCategory) {
    var folder = GetProgramsFolderToEdit(programCategory);
    var programFiles = folder.GetFiles("*" + Utility.ProgramExtension);
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

  private string GetSignalProcessorCcTemplate() {
    using var reader = new StringReader(MainCcTemplate);
    ConnectionsStartLine = reader.ReadLine()!;
    string inputSignalConnectionLine = reader.ReadLine()!;
    string result = inputSignalConnectionLine.Replace(
      "Destination=\"Value\"", "Destination=\"Macro1\"");
    ConnectionsEndLine = reader.ReadLine()!;
    return result;
  }

  private string GetTemplateProgramPath() {
    var templateProgramFile = new FileInfo(
      Path.Combine(InstrumentProgramsFolder.FullName,
        TemplateProgramCategory, TemplateProgramName + Utility.ProgramExtension));
    if (!templateProgramFile.Exists) {
      Console.Error.WriteLine($"Cannot find file '{templateProgramFile.FullName}'.");
      Environment.Exit(1);
    }
    return templateProgramFile.FullName;
  }

  private void Initialise() {
    InstrumentProgramsFolder = GetInstrumentProgramsFolder();
    TemplateProgramPath = GetTemplateProgramPath();
    MainCcTemplate = GetMainCcTemplate();
    SignalProcessorCcTemplate = GetSignalProcessorCcTemplate();
  }

  /// <summary>
  ///   Returns the specified text updated to replace the CC number in the
  ///   SignalConnection tag with the specified CC number.
  ///   E.g. replaces 
  ///     Source="@MIDI CC 31"
  ///   with
  ///     Source="@MIDI CC 123"
  /// </summary>
  private static string ReplaceMidiCcNo(string oldText, int newCcNo) {
    int ccNoToUse = newCcNo != 35 ? newCcNo : 11;
    const string pattern = "Source=\"@MIDI CC \\d+\"";
    string replacement = "Source=\"@MIDI CC " + ccNoToUse + "\"";
    return Regex.Replace(oldText, pattern, replacement);

  }

  protected virtual void UpdateCCs(string programPath) {
    // Dual XML data load strategy:
    // To maximise forward compatibility with possible future changes to the program XML
    // data structure, we are deserialising only nodes we need, to the
    // ConstantModulations and ScriptProcessors lists. So we cannot serialise back to
    // file from those lists. Instead, the program XML file must be updated via
    // LINQ to XML in ProgramXml. 
    DeserialiseProgramXml(programPath);
    ProgramXml = new ProgramXml(TemplateProgramPath);
    ProgramXml.LoadFromFile(programPath);
    MacroCcsScriptProcessor = FindMacroCcsScriptProcessor(); 
    if (MacroCcsScriptProcessor != null) {
      Console.WriteLine($"Macro CCs ScriptProcessor in '{programPath}'.");
      UpdateMacroCcsInScriptProcessor();
    } else {
      UpdateMacroCcsInConstantModulations();
    }
    ProgramXml.SaveToFile(programPath);
  }

  /// <summary>
  /// Updates the macro CCs so that, top to bottom, left to right on the Info page,
  /// the macros are successively assigned standard CCs in ascending order,
  /// with different series of CCs for continuous and switch macros.
  /// </summary>
  private void UpdateMacroCcsInConstantModulations() {
    // Most Factory programs list the ConstantModulation macro specifications in order
    // top to bottom, left to right.  But a few, e.g. Keys/Days Of Old 1.4, do not.
    // 
    // In the Devinity sound bank, the XML lists the
    // macros out of order and some macros do not appear on the info page.
    // (I have not yet been able to work out how to make a macro not appear on the Info
    // page or how tell from the XML whether it is or is not on the Info page.)
    //
    int nextContinuousCcNo = 31;
    int nextSwitchCcNo = 112;
    for (int index = 0; index < ConstantModulations.Count; index++) {
      var constantModulation = ConstantModulations[index];
      int ccNo;
      if (constantModulation.IsContinuous) {
        ccNo = nextContinuousCcNo;
        nextContinuousCcNo++;
      } else {
        ccNo = nextSwitchCcNo;
        nextSwitchCcNo++;
      }
      if (constantModulation.SignalConnections.Count == 0) { // Will be 0 or 1
        // The macro is not already mapped to a CC number.
        var signalConnection = new SignalConnection {
          CcNo = ccNo,
          Destination = "Value"
        };
        ProgramXml.AddConstantModulationSignalConnection(signalConnection, index);
      } else {
        // The macro already has a Connections block mapping to a CC.
        // We need to conserve the SignalConnection tag, which might contain a custom
        // Ratio, and just replace the CC number.
        var signalConnection = constantModulation.SignalConnections[0];
        ProgramXml.UpdateConstantModulationSignalConnection(signalConnection, index);
      }
    }
  }

  private void UpdateMacroCcsInScriptProcessor() {
    int macroCount = ConstantModulations.Count;
    // So far, all Factory programs with macro CCs specified in the
    // ScriptProcessor have no switch macros and are listed in the ConstantModulations
    // in top to bottom order.  But if this is made generic, it may work for many other
    // sound banks.
    int nextContinuousCcNo = 31;
    MacroCcsScriptProcessor!.SignalConnections.Clear();
    for (int macroNo = 1; macroNo <= macroCount; macroNo++) {
      MacroCcsScriptProcessor.SignalConnections.Add(
        new SignalConnection {
          MacroNo = macroNo,
          CcNo = nextContinuousCcNo
        });
      nextContinuousCcNo++;
    }
    ProgramXml.UpdateMacroCcsScriptProcessor(MacroCcsScriptProcessor);
  }
}