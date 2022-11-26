using System.Text.RegularExpressions;

namespace FalconProgrammer;

public static class Utility {
  private const string InstrumentName = "Organic Keys";
  private const string ProgramExtension = ".uvip";
  private const string ProgramTypeName = "More Than Keys";
  private const string PublisherName = "UVI";
  private const string ScriptProcessorName = "EventProcessor0";
  private const string TemplateProgramName = "A Rhapsody";
  private const string TemplateProgramTypeName = "Acoustic Mood";
  private const string TemplateScriptProcessorName = "EventProcessor0";
  
  private static DirectoryInfo InstrumentProgramsFolder { get; set; } = null!;
  private static string TemplateCcConfiguration { get; set; } = null!;
  private static string TemplateProgramPath { get; set; } = null!;

  /// <summary>
  ///   Configures macro CCs for Falcon program presets where the Info page is defined in
  ///   a script.
  /// </summary>
  public static void ConfigureScriptCcs() {
    InstrumentProgramsFolder = GetInstrumentProgramsFolder();
    TemplateProgramPath = GetTemplateProgramPath();
    TemplateCcConfiguration = GetTemplateCcConfiguration();
    var programFilesToEdit = GetProgramFilesToEdit();
    foreach (var programFileToEdit in programFilesToEdit) {
      UpdateCCs(programFileToEdit.FullName);
    }
    Console.WriteLine("Finished");
  }

  private static DirectoryInfo GetInstrumentProgramsFolder() {
    var publisherSoftwareFolder = new DirectoryInfo(
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "Music", "Software", PublisherName));
    if (!publisherSoftwareFolder.Exists) {
      Console.Error.WriteLine(
        $"Cannot find folder '{publisherSoftwareFolder.FullName}'.");
      Environment.Exit(1);
    }
    var result = new DirectoryInfo(
      Path.Combine(
        publisherSoftwareFolder.FullName,
        "Falcon", "Programs", InstrumentName));
    if (!result.Exists) {
      Console.Error.WriteLine($"Cannot find folder '{result.FullName}'.");
      Environment.Exit(1);
    }
    return result;
  }

  private static IEnumerable<FileInfo> GetProgramFilesToEdit() {
    var folder = GetProgramsFolderToEdit();
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

  private static DirectoryInfo GetProgramsFolderToEdit() {
    var result = new DirectoryInfo(
      Path.Combine(InstrumentProgramsFolder.FullName, ProgramTypeName));
    if (!result.Exists) {
      Console.Error.WriteLine($"Cannot find folder '{result.FullName}'.");
      Environment.Exit(1);
    }
    return result;
  }

  private static string GetTemplateCcConfiguration() {
    using var reader = new StreamReader(TemplateProgramPath);
    string line = string.Empty;
    while (!reader.EndOfStream) {
      line = reader.ReadLine()!;
      if (line.Contains(TemplateScriptProcessorName)) {
        break;
      }
    }
    if (!line.Contains(TemplateScriptProcessorName)) {
      Console.Error.WriteLine(
        $"Cannot find {TemplateScriptProcessorName} in file '{TemplateProgramPath}'.");
      Environment.Exit(1);
    }
    if (!reader.EndOfStream) {
      line = reader.ReadLine()!;
    }
    using var writer = new StringWriter();
    if (line.Contains("<Connections>")) {
      writer.WriteLine(line);
    } else {
      Console.Error.WriteLine(
        $"Cannot find start of node EventProcessor0.Connections in file '{TemplateProgramPath}'.");
      Environment.Exit(1);
    }
    while (!reader.EndOfStream) {
      line = reader.ReadLine()!;
      writer.WriteLine(line);
      if (line.Contains("</Connections>")) {
        break;
      }
    }
    return writer.ToString();
  }

  private static string GetTemplateProgramPath() {
    var templateProgramFile = new FileInfo(
      Path.Combine(InstrumentProgramsFolder.FullName,
        TemplateProgramTypeName, TemplateProgramName + ProgramExtension));
    if (!templateProgramFile.Exists) {
      Console.Error.WriteLine($"Cannot find file '{templateProgramFile.FullName}'.");
      Environment.Exit(1);
    }
    return templateProgramFile.FullName;
  }

  private static void UpdateCCs(string programPath) {
    Console.WriteLine($"Updating '{programPath}'.");
    string inputText;
    using (var fileReader = new StreamReader(programPath)) {
      inputText = fileReader.ReadToEnd();
    }
    using var reader = new StringReader(inputText);
    using var writer = new StreamWriter(programPath);
    string line = string.Empty;
    while (reader.Peek() >= 0) {
      line = reader.ReadLine()!;
      if (line.Contains(ScriptProcessorName)) {
        // Initialise Delay and Reverb to zero.
        // E.g. replaces 
        //    delaySend="0.13236231" reverbSend="0.36663184"
        // with
        //    delaySend="0" reverbSend="0"
        const string pattern = "delaySend=\"0\\.\\d+\" reverbSend=\"0\\.\\d+\"";
        const string replacement = "delaySend=\"0\" reverbSend=\"0\"";
        string zeroedScriptProcessorLine = Regex.Replace(line, pattern, replacement);
        writer.WriteLine(zeroedScriptProcessorLine);
        break;
      }
      writer.WriteLine(line);
    }
    if (!line.Contains(TemplateScriptProcessorName)) {
      Console.Error.WriteLine(
        $"Cannot find {ScriptProcessorName} in file '{programPath}'.");
      Environment.Exit(1);
    }
    if (reader.Peek() >= 0) {
      line = reader.ReadLine()!;
    }
    if (line.Contains("<Connections>")) {
      // Skip the existing CC configuration
      while (reader.Peek() >= 0) {
        line = reader.ReadLine()!;
        if (line.Contains("</Connections>")) {
          line = reader.ReadLine()!;
          break;
        }
      }
    }
    writer.Write(TemplateCcConfiguration);
    writer.WriteLine(line); // Properties tag
    writer.Write(reader.ReadToEnd());
  }
}