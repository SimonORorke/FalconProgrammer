using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

/// <summary>
///   A category of Falcon programs, with the program files stored in a folder with the
///   category name.
/// </summary>
public class Category {
  public Category(DirectoryInfo soundBankFolder, string name, Settings settings) {
    SoundBankFolder = soundBankFolder;
    Name = name;
    Settings = settings;
    Path = $"{SoundBankFolder.Name}\\{Name}";
  }

  private DirectoryInfo Folder { get; set; } = null!;

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
  public ProgramXml ProgramXml { get; set; } = null!;
  [PublicAPI] public Settings Settings { get; }
  // private Settings.ProgramCategory SettingsCategory { get; set; } = null!;
  public DirectoryInfo SoundBankFolder { get; }
  [PublicAPI] public string TemplateCategoryName => TemplateProgramFile.Directory!.Name;

  [PublicAPI]
  public string TemplateProgramName =>
    System.IO.Path.GetFileNameWithoutExtension(TemplateProgramPath);

  private FileInfo TemplateProgramFile { get; set; } = null!;
  public string TemplateProgramPath => TemplateProgramFile.FullName;

  /// <summary>
  ///   For programs where the Info page layout is specified in a script, the template
  ///   ScriptProcessor contains the Modulations that map the macros to MIDI CC
  ///   numbers.
  /// </summary>
  public ScriptProcessor? TemplateScriptProcessor { get; private set; }

  [PublicAPI]
  public string TemplateSoundBankName => 
    Directory.GetParent(TemplateProgramFile.Directory!.FullName)?.Name!;

  private DirectoryInfo GetFolder(string categoryName) {
    var result = new DirectoryInfo(
      System.IO.Path.Combine(SoundBankFolder.FullName, categoryName));
    if (!result.Exists) {
      throw new InvalidOperationException(
        $"Category {Path}: Cannot find category folder '{result.FullName}'.");
    }
    return result;
  }

  public IEnumerable<FileInfo> GetProgramFilesToEdit() {
    var programFiles = Folder.GetFiles(
      "*" + Batch.ProgramExtension);
    var result = (
      from programFile in programFiles
      where programFile.FullName != TemplateProgramPath
      select programFile).ToList();
    if (result.Count == 0) {
      throw new InvalidOperationException(
        $"Category {Path}: There are no program files to edit in folder '{Folder.FullName}'.");
    }
    return result;
  }

  public FileInfo GetProgramFile(string programName) {
    var result = new FileInfo(
      System.IO.Path.Combine(Folder.FullName, $"{programName}.uvip"));
    if (!result.Exists) {
      throw new InvalidOperationException(
        $"Category {Path}: Cannot find program file '{result.FullName}'.");
    }
    return result;
  }
  
  private FileInfo GetTemplateProgramFile() {
    string templatesFolderPath = Batch.GetTemplateProgramsFolder().FullName; 
    string categoryTemplateFolderPath = System.IO.Path.Combine(
      templatesFolderPath, SoundBankFolder.Name, Name);
    var folder = new DirectoryInfo(categoryTemplateFolderPath);
    if (!folder.Exists) {
      folder = null;
      string soundBankTemplateFolderPath = System.IO.Path.Combine(
        templatesFolderPath, SoundBankFolder.Name);
      var soundBankTemplateFolder = new DirectoryInfo(soundBankTemplateFolderPath);
      if (soundBankTemplateFolder.Exists) {
        var subFolders = 
          soundBankTemplateFolder.EnumerateDirectories().ToList();
        if (subFolders.Count == 1) {
          folder = subFolders[0];
        }
      }
    }
    if (folder != null) {
      var templateFile = (
        from programFile in folder.EnumerateFiles("*.uvip")
        select programFile).FirstOrDefault();
      if (templateFile != null) {
        return templateFile;
      }
    }
    if (string.IsNullOrEmpty(Settings.DefaultTemplate.SubPath)) {
      throw new InvalidOperationException(
        $"Category {Path}: A default Template must be specified the " + 
        "Settings file, to specify TemplateScriptProcessor.");
    }
    string defaultTemplatePath = System.IO.Path.Combine(
      templatesFolderPath, Settings.DefaultTemplate.SubPath);
    var defaultTemplateFile = new FileInfo(defaultTemplatePath);
    if (!defaultTemplateFile.Exists) {
      throw new InvalidOperationException(
        $"Category {Path}: Cannot find default template file '{defaultTemplatePath}'.");
    }
    return defaultTemplateFile;
  }

  /// <summary>
  ///   For programs where the Info page layout is specified in a script, the template
  ///   ScriptProcessor is assumed to be the last ScriptProcessor in the program, in this
  ///   case the template program.
  /// </summary>
  private ScriptProcessor? GetTemplateScriptProcessor() {
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
    Folder = GetFolder(Name);
    // SettingsCategory = Settings.MustUseGuiScriptProcessor(SoundBankFolder.Name, Name);
    TemplateProgramFile = GetTemplateProgramFile();
    TemplateScriptProcessor = GetTemplateScriptProcessor();
  }
}