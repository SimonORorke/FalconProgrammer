using JetBrains.Annotations;

namespace FalconProgrammer;

public class ProgramConfig {
  public ProgramConfig(
    string instrumentName, string templateProgramCategory, string templateProgramName) {
    InstrumentName = instrumentName;
    TemplateProgramCategory = templateProgramCategory;
    TemplateProgramName = templateProgramName;
  }

  [PublicAPI] public string InstrumentName { get; }
  [PublicAPI] public DirectoryInfo InstrumentProgramsFolder { get; private set; } = null!;
  [PublicAPI] public string TemplateProgramCategory { get; }
  [PublicAPI] public string TemplateProgramName { get; }
  [PublicAPI] public string TemplateCcConfig { get; private set; } = null!;
  [PublicAPI] public string TemplateProgramPath { get; private set; } = null!;

  /// <summary>
  ///   Configures macro CCs for Falcon program presets.
  /// </summary>
  public void ConfigureCcs(string programCategory) {
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

  protected virtual string GetTemplateCcConfig() {
    throw new NotImplementedException();
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
    TemplateCcConfig = GetTemplateCcConfig();
  }

  protected virtual void UpdateCCs(string programPath) {
    throw new NotImplementedException();
  }
}