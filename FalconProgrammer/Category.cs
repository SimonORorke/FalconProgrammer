using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
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
  }

  private DirectoryInfo Folder { get; set; } = null!;

  /// <summary>
  ///   I think the only categories where the info page script processor cannot be
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
  ///     SignalConnections mapping MIDI CC numbers to macros must be added to that
  ///     script's ScriptProcessor. The SignalConnections are copied from a template
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
  public bool InfoPageMustUseScript => SettingsCategory.InfoPageMustUseScript;

  [PublicAPI] public string Name { get; }
  [PublicAPI] public Settings Settings { get; }
  private Settings.ProgramCategory SettingsCategory { get; set; } = null!;
  public DirectoryInfo SoundBankFolder { get; }
  [PublicAPI] public string TemplateCategoryName => TemplateProgramFile.Directory!.Name;

  [PublicAPI]
  public string TemplateProgramName =>
    Path.GetFileNameWithoutExtension(TemplateProgramPath);

  private FileInfo TemplateProgramFile { get; set; } = null!;
  public string TemplateProgramPath => TemplateProgramFile.FullName;

  /// <summary>
  ///   For programs where the Info page layout is specified in a script, the template
  ///   ScriptProcessor contains the SignalConnections that map the macros to MIDI CC
  ///   numbers.
  /// </summary>
  public ScriptProcessor? TemplateScriptProcessor { get; private set; }

  [PublicAPI]
  public string TemplateSoundBankName =>
    Directory.GetParent(TemplateProgramFile.Directory!.FullName)?.Name!;

  private DirectoryInfo GetFolder(string categoryName) {
    var result = new DirectoryInfo(
      Path.Combine(SoundBankFolder.FullName, categoryName));
    if (!result.Exists) {
      throw new ApplicationException(
        $"Cannot find category folder '{result.FullName}'.");
    }
    return result;
  }

  public IEnumerable<FileInfo> GetProgramFilesToEdit() {
    var programFiles = Folder.GetFiles(
      "*" + BatchProcessor.ProgramExtension);
    var result = (
      from programFile in programFiles
      where programFile.FullName != TemplateProgramPath
      select programFile).ToList();
    if (result.Count == 0) {
      throw new ApplicationException(
        $"There are no program files to edit in folder '{Folder.FullName}'.");
    }
    return result;
  }

  private FileInfo GetTemplateProgramFile() {
    var result = new FileInfo(SettingsCategory.TemplatePath);
    if (!result.Exists) {
      throw new ApplicationException(
        $"Cannot find template file '{SettingsCategory.TemplatePath}'.");
    }
    return result;
  }

  /// <summary>
  ///   For programs where the Info page layout is specified in a script, the template
  ///   ScriptProcessor is assumed to be the last ScriptProcessor in the program, in this
  ///   case the template program.
  /// </summary>
  private ScriptProcessor? GetTemplateScriptProcessor() {
    using var reader = new StreamReader(TemplateProgramPath);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    var templateScriptProcessor = (
      from scriptProcessor in root.Program.ScriptProcessors
      select scriptProcessor).LastOrDefault();
    if (templateScriptProcessor == null && InfoPageMustUseScript) {
      throw new ApplicationException(
        $"Cannot find ScriptProcessor in file '{TemplateProgramPath}'.");
    }
    return templateScriptProcessor;
  }

  public void Initialise() {
    Folder = GetFolder(Name);
    SettingsCategory = Settings.GetProgramCategory(SoundBankFolder.Name, Name);
    TemplateProgramFile = GetTemplateProgramFile();
    TemplateScriptProcessor = GetTemplateScriptProcessor();
  }
}