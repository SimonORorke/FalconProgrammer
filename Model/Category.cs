using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer.Model;

/// <summary>
///   A category of Falcon programs, with the program files stored in a folder with the
///   category name.
/// </summary>
public class Category {
  private IFileSystemService? _fileSystemService;

  public Category(DirectoryInfo soundBankFolder, string name, Settings settings) {
    SoundBankFolder = soundBankFolder;
    Name = name;
    Settings = settings;
    Path = $"{SoundBankFolder.Name}\\{Name}";
  }

  [ExcludeFromCodeCoverage]
  protected virtual IFileSystemService FileSystemService =>
    _fileSystemService ??= Model.FileSystemService.Default;

  private string FolderPath { get; set; } = null!;

  /// <summary>
  ///   I think the only categories where the GUI script processor cannot be
  ///   removed to allow a mod wheel replacement macro are when there are more than 4
  ///   macros. Theoretically that could be done by automatic code analysis, rather than
  ///   with this category setting. But the setting is more efficient, for what it is worth.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     In some sound banks, such as "Organic Keys", ConstantModulations do not specify
  ///     Info page macros, only modulation wheel assignment. In others, such as
  ///     "Hypnotic Drive", ConstantModulation.Properties include the optional attribute
  ///     showValue="0", indicating that the coordinates specified in the Properties will
  ///     not actually be used to determine the locations of macros on the Info page.
  ///     In still others, such as "Factory\Brutal Bass 2.1", the coordinates specified
  ///     in the ConstantModulation.Properties are inaccurate, despite not having the
  ///     showValue="0" attribute.
  ///   </para>
  ///   <para>
  ///     In these cases, the Info page layout is specified in a script.
  ///     Modulations mapping MIDI CC numbers to macros must be added to that
  ///     script's ScriptProcessor. The Modulations are copied from a template
  ///     program file specific to the Info page layout.
  ///   </para>
  ///   <para>
  ///     There is generally one template program file per sound bank, supporting a
  ///     common Info page layout defined in a single script for the whole sound bank.
  ///     However, in the Factory sound bank, there are categories, such as
  ///     "Organic Texture 2.8", or groups of categories for which a script defines the
  ///     Info page layout. In those cases, the template program file has to be specified
  ///     per category.
  ///   </para>
  /// </remarks>
  public bool MustUseGuiScriptProcessor =>
    Settings.MustUseGuiScriptProcessor(SoundBankFolder.Name, Name);

  [PublicAPI] public string Name { get; }
  [PublicAPI] public string Path { get; }
  internal ProgramXml ProgramXml { get; set; } = null!;
  [PublicAPI] public Settings Settings { get; }
  public DirectoryInfo SoundBankFolder { get; }

  [PublicAPI]
  public string TemplateCategoryName => System.IO.Path.GetFileName(  
    System.IO.Path.GetDirectoryName(TemplateProgramPath)!);

  [PublicAPI]
  public string TemplateProgramName =>
    System.IO.Path.GetFileNameWithoutExtension(TemplateProgramPath);

  public string TemplateProgramPath { get; private set; } = null!;

  /// <summary>
  ///   For programs where the Info page layout is specified in a script, the template
  ///   ScriptProcessor contains the Modulations that map the macros to MIDI CC
  ///   numbers.
  /// </summary>
  internal ScriptProcessor? TemplateScriptProcessor { get; private set; }

  [PublicAPI]
  public string TemplateSoundBankName =>
    Directory.GetParent(System.IO.Path.GetDirectoryName(TemplateProgramPath)!)?.Name!;

  private string GetFolderPath(string categoryName) {
    string result = System.IO.Path.Combine(SoundBankFolder.FullName, categoryName);
    if (!FileSystemService.FolderExists(result)) {
      throw new InvalidOperationException(
        $"Category {Path}: Cannot find category folder '{result}'.");
    }
    return result;
  }

  public IEnumerable<string> GetPathsOfProgramFilesToEdit() {
    var programPaths = FileSystemService.GetPathsOfFilesInFolder(
      FolderPath, "*" + Batch.ProgramExtension);
    var result = (
      from programPath in programPaths
      where programPath != TemplateProgramPath
      select programPath).ToList();
    if (result.Count == 0) {
      throw new InvalidOperationException(
        $"Category {Path}: There are no program files to edit in folder '{FolderPath}'.");
    }
    return result;
  }

  public string GetProgramPath(string programName) {
    string result = System.IO.Path.Combine(FolderPath, $"{programName}.uvip");
    if (!FileSystemService.FileExists(result)) {
      throw new InvalidOperationException(
        $"Category {Path}: Cannot find program file '{result}'.");
    }
    return result;
  }

  private string GetTemplateProgramPath() {
    string templatesFolderPath = Settings.TemplateProgramsFolder.Path;
    string categoryTemplateFolderPath = System.IO.Path.Combine(
      templatesFolderPath, SoundBankFolder.Name, Name);
    string folderPath = categoryTemplateFolderPath;
    if (!FileSystemService.FolderExists(folderPath)) {
      folderPath = string.Empty;
      string soundBankTemplateFolderPath = System.IO.Path.Combine(
        templatesFolderPath, SoundBankFolder.Name);
      if (FileSystemService.FolderExists(soundBankTemplateFolderPath)) {
        var subfolderNames =
          FileSystemService.GetSubfolderNames(soundBankTemplateFolderPath);
        if (subfolderNames.Count == 1) {
          folderPath =
            System.IO.Path.Combine(soundBankTemplateFolderPath, subfolderNames[0]);
        }
      }
    }
    if (folderPath != string.Empty) {
      string? templatePath = (
        from programPath in FileSystemService.GetPathsOfFilesInFolder(
          folderPath, "*.uvip")
        select programPath).FirstOrDefault();
      if (templatePath != null) {
        return templatePath;
      }
    }
    if (string.IsNullOrEmpty(Settings.DefaultTemplate.Path)) {
      throw new InvalidOperationException(
        $"Category {Path}: A default Template must be specified in the " +
        "Settings file, to specify TemplateScriptProcessor.");
    }
    if (!FileSystemService.FileExists(Settings.DefaultTemplate.Path)) {
      throw new InvalidOperationException(
        $"Category {Path}: Cannot find default template file " +
        $"'{Settings.DefaultTemplate.Path}'.");
    }
    return Settings.DefaultTemplate.Path;
  }

  /// <summary>
  ///   For programs where the Info page layout is specified in a script, the template
  ///   ScriptProcessor is assumed to be the last ScriptProcessor in the program, in this
  ///   case the template program.
  /// </summary>
  [ExcludeFromCodeCoverage]
  protected virtual ScriptProcessor? GetTemplateScriptProcessor() {
    var templateXml = new ProgramXml(this);
    templateXml.LoadFromFile(TemplateProgramPath);
    // Testing for macro-modulating Modulations might be more reliable than
    // assuming the last ScriptProcessor.  But I think I tried that and there was 
    // a problem, don't know what though.  Can be revisited if assuming the last
    // ScriptProcessor turns out not to be reliable.  But I think that's actually fine
    // in all cases.
    if (templateXml.TemplateScriptProcessorElement != null) {
      return ScriptProcessor.Create(
        SoundBankFolder.Name, templateXml.TemplateScriptProcessorElement, ProgramXml);
    }
    if (!MustUseGuiScriptProcessor) {
      return null;
    }
    throw new InvalidOperationException(
      $"Category {Path}: Cannot find ScriptProcessor in file '{TemplateProgramPath}'.");
  }

  public void Initialise() {
    FolderPath = GetFolderPath(Name);
    TemplateProgramPath = GetTemplateProgramPath();
    TemplateScriptProcessor = GetTemplateScriptProcessor();
  }
}