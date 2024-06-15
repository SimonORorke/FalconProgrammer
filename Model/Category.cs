using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using FalconProgrammer.Model.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer.Model;

/// <summary>
///   A category of Falcon programs, with the program files stored in a folder with the
///   category name.
/// </summary>
internal class Category {
  private IFileSystemService? _fileSystemService;

  public Category(string soundBankFolderPath, string name, Settings settings) {
    SoundBankFolderPath = soundBankFolderPath;
    Name = name;
    Settings = settings;
    PathShort = $@"{SoundBankName}\{Name}";
  }

  [ExcludeFromCodeCoverage]
  protected virtual IFileSystemService FileSystemService =>
    _fileSystemService ??= Model.FileSystemService.Default;

  internal string Path => System.IO.Path.Combine(
    Settings.ProgramsFolder.Path, SoundBankName, Name);

  /// <summary>
  ///   TODO: Revise MustUseGuiScriptProcessor documentation.
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
  ///     In still others, such as "Falcon Factory\Brutal Bass 2.1", the coordinates specified
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
  ///     However, in the Falcon Factory sound bank, there are categories, such as
  ///     "Organic Texture 2.8", or groups of categories for which a script defines the
  ///     Info page layout. In those cases, the template program file has to be specified
  ///     per category.
  ///   </para>
  /// </remarks>
  public bool MustUseGuiScriptProcessor =>
    Settings.MustUseGuiScriptProcessor(SoundBankName, Name);

  public string Name { get; }
  [PublicAPI] public string PathShort { get; }
  internal ProgramXml ProgramXml { get; set; } = null!;
  [PublicAPI] public Settings Settings { get; }
  private string SoundBankFolderPath { get; }
  public string SoundBankName => System.IO.Path.GetFileName(SoundBankFolderPath);

  public string TemplateCategoryName => System.IO.Path.GetFileName(
    System.IO.Path.GetDirectoryName(TemplateProgramPath)!);

  public string? TemplateProgramName =>
    System.IO.Path.GetFileNameWithoutExtension(TemplateProgramPath);

  public string? TemplateProgramPath { get; protected set; }

  [PublicAPI]
  public string? TemplateSoundBankName =>
    Directory.GetParent(System.IO.Path.GetDirectoryName(TemplateProgramPath)!)?.Name;

  [ExcludeFromCodeCoverage]
  protected virtual FalconProgram CreateTemplateProgram(Batch batch) {
    return new FalconProgram(TemplateProgramPath!, this, batch);
  }

  /// <summary>
  ///   Finds the ScriptProcessor, if any, that is to contain the Modulations that
  ///   map the macros to MIDI CC numbers. If the ScriptProcessor is not found, each
  ///   macro's MIDI CC number must be defined in a Modulation owned by the
  ///   macro's ConstantModulation.
  /// </summary>
  [SuppressMessage("ReSharper", "CommentTypo")]
  public static ScriptProcessor? FindGuiScriptProcessor(
    IList<ScriptProcessor> scriptProcessors) {
    if (scriptProcessors.Count == 0) {
      return null;
    }
    foreach (var scriptProcessor in scriptProcessors) {
      switch (scriptProcessor.GuiScriptId) {
        case ScriptId.SoundBank1:
        case ScriptId.SoundBank2:
        case ScriptId.Factory2_1:
        case ScriptId.Factory2_5:
        case ScriptId.FactoryRev2:
        case ScriptId.OrganicTexture:
        case ScriptId.Main1:
        case ScriptId.Main2:
          return scriptProcessor;
      }
      if (scriptProcessor is { SoundBankId: "FalconFactory", Name: "EventProcessor9" }) {
        // Examples of programs with GuiScriptProcessor but no template ScriptProcessor:
        // Falcon Factory\Bass-Sub\Balarbas 2.0
        // Falcon Factory\Keys\Smooth E-piano 2.1.
        // However, these are all in categories that also contain programs that do not
        // have a GUI script processor. And currently MustUseGuiScriptProcessor is not
        // supported for categories where not all prorams have a GUI script processor.
        // If the user tries it, UpdateMacroCcs will throw an application.
        // So currently these GUI script processors will always be removed by
        // InitialiseLayout. We are indicating them as GUI script processors precisely
        // so that they will be removed as unusable.
        return scriptProcessor;
      }
    }
    return null;
  }

  private string? FindTemplateProgramPath() {
    if (string.IsNullOrEmpty(Settings.TemplateProgramsFolder.Path)) {
      return null;
    }
    ValidateTemplateProgramsFolderPath();
    string templatesFolderPath = Settings.TemplateProgramsFolder.Path;
    string categoryTemplateFolderPath = System.IO.Path.Combine(
      templatesFolderPath, SoundBankName, Name);
    string folderPath = categoryTemplateFolderPath;
    if (!FileSystemService.Folder.Exists(folderPath)) {
      folderPath = string.Empty;
      string soundBankTemplateFolderPath = System.IO.Path.Combine(
        templatesFolderPath, SoundBankName);
      if (FileSystemService.Folder.Exists(soundBankTemplateFolderPath)) {
        var subfolderNames =
          FileSystemService.Folder.GetSubfolderNames(soundBankTemplateFolderPath);
        if (subfolderNames.Count == 1) {
          folderPath =
            System.IO.Path.Combine(soundBankTemplateFolderPath, subfolderNames[0]);
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
    return null;
  }

  public IEnumerable<string> GetPathsOfProgramFilesToEdit() {
    var programPaths = FileSystemService.Folder.GetFilePaths(
      Path, "*.uvip");
    var result = (
      from programPath in programPaths
      where programPath != TemplateProgramPath
      select programPath).ToList();
    if (result.Count == 0) {
      throw new ApplicationException(
        $"Category {PathShort}: " +
        $"There are no program files to edit in folder '{Path}'.");
    }
    return result;
  }

  public string GetProgramPath(string programName) {
    string result = System.IO.Path.Combine(Path, $"{programName}.uvip");
    if (!FileSystemService.File.Exists(result)) {
      throw new ApplicationException(
        $"Category {PathShort}: Cannot find program file '{result}'.");
    }
    return result;
  }

  public ScriptProcessor GetTemplateScriptProcessor(
    ScriptProcessor guiScriptProcessor, Batch batch) {
    return TemplateProgramPath != null
      ? GetTemplateScriptProcessorFromFile(batch)
      : GetTemplateScriptProcessorFromEmbeddedFile(guiScriptProcessor);
  }

  private ScriptProcessor GetTemplateScriptProcessorFromEmbeddedFile(
    ScriptProcessor guiScriptProcessor) {
    string? embeddedFileName = guiScriptProcessor.GuiScriptId switch {
      ScriptId.Factory2_1 => "Factory_2_1_Gui.xml",
      ScriptId.Factory2_5 => "Factory_2_5_Gui.xml",
      ScriptId.OrganicTexture => "Factory_OrganicTexture_Gui.xml",
      _ => null
    };
    if (embeddedFileName == null
        && guiScriptProcessor.GuiScriptId != ScriptId.None) {
      embeddedFileName = guiScriptProcessor.SoundBankId switch {
        // The Hypnotic Drive program files have a typo in the script sound bank name.
        "HypnoticDive" => "HypnoticDrive_Gui.xml",
        "Pulsar" => 
          $"{guiScriptProcessor.SoundBankId}_{guiScriptProcessor.Category}_Gui.xml",
        _ => $"{guiScriptProcessor.SoundBankId}_Gui.xml"
      };
    }
    if (embeddedFileName != null) {
      var template = new ScriptProcessorEmbeddedXml(embeddedFileName);
      return ScriptProcessor.Create(SoundBankName,
        new XElement(template.ScriptProcessorElement),
        ProgramXml, Settings.MidiForMacros);
    }
    throw new ApplicationException(
      "A built-in GUI ScriptProcessor template is not available for " 
      + $"sound bank '{SoundBankName}' category '{Name}'. " +
      $"You have two options:{Environment.NewLine}{Environment.NewLine}" +
      "1) Add a template program file with MIDI CCs for the sound bank or category to " +
      "the Template Programs folder. " +
      $" (See the manual for instructions.){Environment.NewLine}{Environment.NewLine}" +
      "2) Remove the sound bank or category from the list on the GUI Script Processor " +
      "page and run InitialiseLayout to remove the script-defined GUI before " +
      $"running UpdateMacroCcs.{Environment.NewLine}{Environment.NewLine}" +
      "Also, consider asking the developers to implement a built-in " +
      "template for the sound bank or category.");
  }

  /// <summary>
  ///   For programs where the Info page layout is specified in a script, the template
  ///   ScriptProcessor is assumed to be the last ScriptProcessor in the program, in this
  ///   case the template program.
  /// </summary>
  private ScriptProcessor GetTemplateScriptProcessorFromFile(Batch batch) {
    var templateProgram = CreateTemplateProgram(batch);
    templateProgram.Read();
    var result = FindGuiScriptProcessor(templateProgram.ScriptProcessors);
    if (result != null) {
      return result;
    }
    throw new ApplicationException(
      $"Category '{PathShort}': " +
      "Cannot find the GUI ScriptProcessor in template program file '" +
      $"{TemplateProgramPath}'. " +
      $"You have three options:{Environment.NewLine}{Environment.NewLine}" +
      "1) Replace the template program file with one that contains a " +
      "GUI ScriptProcessor with MIDI CCs specified for the macros. " +
      $" (See the manual for details.){Environment.NewLine}{Environment.NewLine}" +
      "2) Remove the template program file and use the built-in GUI ScriptProcessor " +
      "template for the sound bank or category, if one is available." +
      $"{Environment.NewLine}{Environment.NewLine}" +
      "3) Remove the sound bank or category from the list on the GUI Script Processor " +
      "page and run InitialiseLayout to remove the script-defined GUI before " +
      "running UpdateMacroCcs.");
  }

  public void Initialise() {
    if (!FileSystemService.Folder.Exists(Path)) {
      throw new ApplicationException(
        $"Category '{PathShort}': Cannot find category folder '{Path}'.");
    }
    TemplateProgramPath = FindTemplateProgramPath();
  }

  private void ValidateTemplateProgramsFolderPath() {
    if (!FileSystemService.Folder.Exists(Settings.TemplateProgramsFolder.Path)) {
      throw new ApplicationException(
        "Cannot find template programs folder " +
        $"'{Settings.TemplateProgramsFolder.Path}'.");
    }
  }
}