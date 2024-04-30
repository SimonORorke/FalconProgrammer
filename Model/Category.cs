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

  public Category(string soundBankFolderPath, string name, Settings settings) {
    SoundBankFolderPath = soundBankFolderPath;
    Name = name;
    Settings = settings;
    PathShort = $"{SoundBankName}\\{Name}";
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
    Settings.MustUseGuiScriptProcessor(SoundBankName, Name);

  [PublicAPI] public string Name { get; }
  [PublicAPI] public string PathShort { get; }
  internal ProgramXml ProgramXml { get; set; } = null!;
  [PublicAPI] public Settings Settings { get; }
  private string SoundBankFolderPath { get; }
  public string SoundBankName => Path.GetFileName(SoundBankFolderPath);

  [PublicAPI]
  public string TemplateCategoryName => Path.GetFileName(
    Path.GetDirectoryName(TemplateProgramPath)!);

  [PublicAPI]
  public string TemplateProgramName =>
    Path.GetFileNameWithoutExtension(TemplateProgramPath);

  public string TemplateProgramPath { get; private set; } = null!;

  /// <summary>
  ///   For programs where the Info page layout is specified in a script, the template
  ///   ScriptProcessor contains the Modulations that map the macros to MIDI CC
  ///   numbers.
  /// </summary>
  internal ScriptProcessor? TemplateScriptProcessor { get; private set; }

  [PublicAPI]
  public string TemplateSoundBankName =>
    Directory.GetParent(Path.GetDirectoryName(TemplateProgramPath)!)?.Name!;

  private string GetFolderPath(string categoryName) {
    string result = Path.Combine(SoundBankFolderPath, categoryName);
    if (!FileSystemService.Folder.Exists(result)) {
      throw new ApplicationException(
        $"Category {PathShort}: Cannot find category folder '{result}'.");
    }
    return result;
  }

  public IEnumerable<string> GetPathsOfProgramFilesToEdit() {
    var programPaths = FileSystemService.Folder.GetFilePaths(
      FolderPath, "*.uvip");
    var result = (
      from programPath in programPaths
      where programPath != TemplateProgramPath
      select programPath).ToList();
    if (result.Count == 0) {
      throw new ApplicationException(
        $"Category {PathShort}: There are no program files to edit in folder '{FolderPath}'.");
    }
    return result;
  }

  public string GetProgramPath(string programName) {
    string result = Path.Combine(FolderPath, $"{programName}.uvip");
    if (!FileSystemService.File.Exists(result)) {
      throw new ApplicationException(
        $"Category {PathShort}: Cannot find program file '{result}'.");
    }
    return result;
  }

  private string GetTemplateProgramPath() {
    ValidateTemplateProgramsFolderPath();
    string templatesFolderPath = Settings.TemplateProgramsFolder.Path;
    string categoryTemplateFolderPath = Path.Combine(
      templatesFolderPath, SoundBankName, Name);
    string folderPath = categoryTemplateFolderPath;
    if (!FileSystemService.Folder.Exists(folderPath)) {
      folderPath = string.Empty;
      string soundBankTemplateFolderPath = Path.Combine(
        templatesFolderPath, SoundBankName);
      if (FileSystemService.Folder.Exists(soundBankTemplateFolderPath)) {
        var subfolderNames =
          FileSystemService.Folder.GetSubfolderNames(soundBankTemplateFolderPath);
        if (subfolderNames.Count == 1) {
          folderPath =
            Path.Combine(soundBankTemplateFolderPath, subfolderNames[0]);
        }
      }
    }
    if (folderPath != string.Empty) {
      string? templatePath = (
        from programPath in FileSystemService.Folder.GetFilePaths(
          folderPath, "*.uvip")
        select programPath).FirstOrDefault();
      if (templatePath != null) {
        return templatePath;
      }
    }
    if (string.IsNullOrEmpty(Settings.DefaultTemplate.Path)) {
      throw new ApplicationException(
        $"Category {PathShort}: A default Template must be specified in the " +
        "Settings file, to specify TemplateScriptProcessor.");
    }
    if (!FileSystemService.File.Exists(Settings.DefaultTemplate.Path)) {
      throw new ApplicationException(
        $"Category {PathShort}: Cannot find default template file " +
        $"'{Settings.DefaultTemplate.Path}'.");
    }
    return Settings.DefaultTemplate.Path;
  }

  [ExcludeFromCodeCoverage]
  protected virtual ProgramXml CreateTemplateXml() {
    return new ProgramXml(this);
  }

  /// <summary>
  ///   For programs where the Info page layout is specified in a script, the template
  ///   ScriptProcessor is assumed to be the last ScriptProcessor in the program, in this
  ///   case the template program.
  /// </summary>
  private ScriptProcessor? GetTemplateScriptProcessor() {
    var templateXml = CreateTemplateXml();
    templateXml.LoadFromFile(TemplateProgramPath);
    // Testing for macro-modulating Modulations might be more reliable than
    // assuming the last ScriptProcessor.  But I think I tried that and there was 
    // a problem, don't know what though.  Can be revisited if assuming the last
    // ScriptProcessor turns out not to be reliable.  But I think that's actually fine
    // in all cases.
    if (templateXml.TemplateScriptProcessorElement != null) {
      return ScriptProcessor.Create(
        SoundBankName, templateXml.TemplateScriptProcessorElement, ProgramXml);
    }
    if (!MustUseGuiScriptProcessor) {
      return null;
    }
    throw new ApplicationException(
      $"Category {PathShort}: Cannot find ScriptProcessor in file '{TemplateProgramPath}'.");
  }

  public void Initialise() {
    FolderPath = GetFolderPath(Name);
    TemplateProgramPath = GetTemplateProgramPath();
    TemplateScriptProcessor = GetTemplateScriptProcessor();
  }

  private void ValidateTemplateProgramsFolderPath() {
    if (string.IsNullOrEmpty(Settings.TemplateProgramsFolder.Path)) {
      throw new ApplicationException(
        "The template programs folder is not specified in settings file " +
        $"'{Settings.SettingsPath}'. If that's not the correct settings file, " +
        "change the settings folder path in " +
        $"'{SettingsFolderLocation.GetSettingsFolderLocationPath()}'.");
    }
    if (!FileSystemService.Folder.Exists(Settings.TemplateProgramsFolder.Path)) {
      throw new ApplicationException(
        "Cannot find template programs folder " +
        $"'{Settings.TemplateProgramsFolder.Path}'.");
    }
  }
}