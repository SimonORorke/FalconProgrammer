using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class ProgramConfig {
  private const string ConnectionsStartTag = "<Connections>";
  private const string ConstantModulationStartTag = "<ConstantModulation ";
  private const string ControlSignalSourcesEndTag = "</ControlSignalSources>";
  private const string EventProcessorsStartTag = "<EventProcessors>";
  private const string PropertiesTag = "<Properties ";
  // Sometimes there's an EventProcessor0 first, e.g. Factory/Keys/Smooth E-piano 2.1.
  // But Info page CC numbers don't go there.
  private const string ScriptProcessorStartTag = 
    "<ScriptProcessor Name=\"EventProcessor9\" "; 
  
  public ProgramConfig(
    string instrumentName, string templateProgramCategory, string templateProgramName) {
    InstrumentName = instrumentName;
    TemplateProgramCategory = templateProgramCategory;
    TemplateProgramName = templateProgramName;
  }

  private string ConnectionsEndLine{ get; set; } = null!;
  private string ConnectionsStartLine{ get; set; } = null!;
  [PublicAPI] public string InstrumentName { get; }
  [PublicAPI] public DirectoryInfo InstrumentProgramsFolder { get; private set; } = null!;
  [PublicAPI] public string MainCcTemplate { get; private set; } = null!;
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

  private static bool HasProgramScriptProcessor(string programPath) {
    using var reader = new StreamReader(programPath);
    string line = string.Empty;
    while (!line.Contains(ControlSignalSourcesEndTag) && reader.Peek() >= 0) {
      line = reader.ReadLine()!;
    }
    if (!line.Contains(ControlSignalSourcesEndTag)) {
      Console.Error.WriteLine(
        $"Cannot find '{ControlSignalSourcesEndTag}' in file '{programPath}'.");
      Environment.Exit(1);
      return false;
    }
    line = reader.ReadLine()!;
    if (!line.Contains(EventProcessorsStartTag)) {
      return false;
    }
    while (!line.Contains(ScriptProcessorStartTag) && reader.Peek() >= 0) {
      line = reader.ReadLine()!;
    }
    return line.Contains(ScriptProcessorStartTag);
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
    // Updates the macro CCs so that, left to right on the Info page,
    // the macros are successively assigned standard CCs in ascending order,
    // with different series of CCs for continuous and switch macros. 
    // For this to work, the macros need to be listed in the XML in their left to right
    // order on the Info page, and all macros need to be shown on the Info page.
    // So it won't work with the Devinity sound bank, where the XML lists the
    // macros out of order and some macros do not appear on the info page.
    // (I have not yet been able to work out how to make a macro not appear on the Info
    // page or how tell from the XML whether it is or is not on the Info page.)
    bool hasProgramScriptProcessor = HasProgramScriptProcessor(programPath); 
    if (hasProgramScriptProcessor) {
      Console.WriteLine($"Program.ScriptProcessor in '{programPath}'.");
      UpdateCCsInScriptProcessor(programPath);
      return;
    }
    string inputText;
    using (var fileReader = new StreamReader(programPath)) {
      inputText = fileReader.ReadToEnd();
    }
    using var reader = new StringReader(inputText);
    using var writer = new StreamWriter(programPath);
    string line = string.Empty;
    while (!line.Contains(ConstantModulationStartTag) && reader.Peek() >= 0) {
      line = reader.ReadLine()!;
      writer.WriteLine(line);
    }
    if (!line.Contains(ConstantModulationStartTag)) {
      Console.Error.WriteLine(
        $"Cannot find '{ConstantModulationStartTag}' in file '{programPath}'.");
      Environment.Exit(1);
      return;
    }
    // We have just written the first line of the ConstantModulation block that defines
    // the first macro.
    int nextContinuousCcNo = 31;
    int nextSwitchCcNo = 112;
    while (!line.Contains(ControlSignalSourcesEndTag) && reader.Peek() >= 0) {
      bool isContinuous;
      if (line.Contains("Style=\"0\"")) {
        isContinuous = true;
      } else if (line.Contains("Style=\"1\"")) {
        isContinuous = false;
      } else {
        Console.Error.WriteLine(
          $"Invalid ConstantModulation start tag line '{line}' in file '{programPath}'.");
        Environment.Exit(1);
        return;
      }
      int ccNo;
      if (isContinuous) {
        ccNo = nextContinuousCcNo;
        nextContinuousCcNo++;
      } else {
        ccNo = nextSwitchCcNo;
        nextSwitchCcNo++;
      }
      line = reader.ReadLine()!; // Connections start tag, if any, otherwise Properties
                                 // tag. 
      if (line.Contains(ConnectionsStartTag)) {
        // The macro already has a Connections block mapping to a CC.
        writer.WriteLine(line); // Write the Connections start tag.
        line = reader.ReadLine()!; // SignalConnection tag line
        // We need to conserve the SignalConnection tag, which might contain a custom
        // Ratio, and just replace the CC number.
        string signalConnectionLine = ReplaceMidiCcNo(line, ccNo);
        writer.WriteLine(signalConnectionLine);
        line = reader.ReadLine()!; // Properties tag line
      } else {
        // The macro is not already mapped to a CC number.
        // So we can insert the template Connections block, which specifies the
        // default Ratio (1), substituting the required CC number.
        string ccConfig = ReplaceMidiCcNo(MainCcTemplate, ccNo);
        writer.Write(ccConfig);
      }
      writer.WriteLine(line); // Properties tag line
      line = reader.ReadLine()!; // ConstantModulation end tag line
      writer.WriteLine(line);
      // If the next block(s), if any, in the ControlSignalSources block is/are not 
      // ConstantModulation(), which configures a macro, (it/they could be LFO(s)
      // instead, for example), write the non-macro block(s) unaltered.
      while (!line.Contains(ConstantModulationStartTag) 
             && !line.Contains(ControlSignalSourcesEndTag)) {
        line = reader.ReadLine()!;
        writer.WriteLine(line);
      }
      // Now we should just have written either the ConstantModulation start tag line
      // of the next macro or the ControlSignalSources end tag line, in which case
      // there are no more macros.
    }
    if (!line.Contains(ControlSignalSourcesEndTag)) {
      Console.Error.WriteLine(
        $"Cannot find '{ControlSignalSourcesEndTag}' in file '{programPath}'.");
      Environment.Exit(1);
      return;
    }
    writer.Write(reader.ReadToEnd());
  }

  private void UpdateCCsInScriptProcessor(string programPath) {
    string inputText;
    using (var fileReader = new StreamReader(programPath)) {
      inputText = fileReader.ReadToEnd();
    }
    using var reader = new StringReader(inputText);
    using var writer = new StreamWriter(programPath);
    writer.AutoFlush = true;
    string line = string.Empty;
    int macroCount = 0;
    while (!line.Contains(ScriptProcessorStartTag) && reader.Peek() >= 0) {
      line = reader.ReadLine()!;
      writer.WriteLine(line);
      if (line.Contains(ConstantModulationStartTag)) {
        macroCount++;
      }
    }
    if (!line.Contains(ScriptProcessorStartTag)) {
      Console.Error.WriteLine(
        $"Cannot find '{ScriptProcessorStartTag}' in file '{programPath}'.");
      Environment.Exit(1);
      return;
    }
    writer.WriteLine(ConnectionsStartLine);
    int nextContinuousCcNo = 31;
    for (int macroNo = 1; macroNo <= macroCount; macroNo++) {
      string signalConnectionLine =
        ReplaceMidiCcNo(SignalProcessorCcTemplate, nextContinuousCcNo)
          .Replace("Macro1", $"Macro{macroNo}");
      writer.WriteLine(signalConnectionLine);
      nextContinuousCcNo++;
    }
    writer.WriteLine(ConnectionsEndLine);
    // Omit any existing Connections block.
    line = reader.ReadLine()!;
    if (line.Contains(ConnectionsStartTag)) {
      while (!line.Contains(PropertiesTag) && reader.Peek() >= 0) {
        line = reader.ReadLine()!;
      }
    }
    writer.WriteLine(line); // Properties line
    writer.Write(reader.ReadToEnd());
  }
}