using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
using JetBrains.Annotations;

namespace FalconProgrammer;

/// <summary>
///   A category of Falcon programs, with the program files stored in a folder with the
///   category name.
/// </summary>
public class Category {
  public Category(string name, DirectoryInfo soundBankFolder) {
    Name = name;
    SoundBankFolder = soundBankFolder;
  }

  private DirectoryInfo Folder { get; set; } = null!;

  /// <summary>
  ///   Name of ScriptProcessor, if any, that is to define the Info page macro CCs.
  /// </summary>
  /// <remarks>
  ///   In the Factory sound bank, sometimes there's an EventProcessor0 first, e.g. in
  ///   Factory/Keys/Smooth E-piano 2.1.
  ///   But Info page CC numbers don't go there.
  /// </remarks>
  public string InfoPageCcsScriptProcessorName {
    get {
      // When the Info page layout is defined in a script, it may be fine to always pick
      // the last ScriptProcessor, which is the fallback in FindInfoPageCcsScriptProcessor.
      // So I will try not bothering to specify any more non-standard ScriptProcessor
      // names. 
      if (IsInfoPageLayoutInScript
          && (SoundBankFolder.Name != "Voklm"
              || Name != "Vox Instruments")) {
        return TemplateScriptProcessorName!;
      }
      // Info page layout is defined in ConstantModulations
      // (and MIDI CC numbers may or may not need to be specified in a ScriptProcessor)
      // or category is "Voklm/Vox Instruments".
      return "EventProcessor9";
    }
  }

  /// <summary>
  ///   In some sound banks, such as "Organic Keys", ConstantModulations do not specify
  ///   Info page macros, only modulation wheel assignment. In others, such as
  ///   "Hypnotic Drive", ConstantModulation.Properties include the optional attribute
  ///   showValue="0", indicating that the coordinates specified in the Properties will
  ///   not actually be used to determine the locations of macros on the Info page.
  ///   In still others, such as "Factory\Brutal Bass 2.1", the coordinates specified in
  ///   the ConstantModulation.Properties are inaccurate, despite not having the
  ///   showValue="0" attribute.
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
  /// </summary>
  public bool IsInfoPageLayoutInScript {
    get {
      switch (SoundBankFolder.Name) {
        case "Hypnotic Drive":
        case "Organic Keys":
        case "Voklm":
          return true;
        case "Factory":
          return Name is "Brutal Bass 2.1" or "Lo-Fi 2.5"
            or "Organic Texture 2.8" or "RetroWave 2.5" or "VCF-20 Synths 2.5";
        default:
          return false;
      }
    }
  }
  
  [PublicAPI] public string Name { get; }
  public DirectoryInfo SoundBankFolder { get; }
  
  [PublicAPI] public string TemplateCategoryName {
    get {
      if (TemplateSoundBankName == "Factory") {
        switch (Name) {
          case "Lo-Fi 2.5" or "RetroWave 2.5" or "VCF-20 Synths 2.5":
            return "Lo-Fi 2.5";
          case "Brutal Bass 2.1" or "Organic Texture 2.8":
            return Name;
        }
      }
      return TemplateSoundBankName switch {
        "Hypnotic Drive" => "Leads",
        "Organic Keys" => "Acoustic Mood",
        "Voklm" => "Synth Choirs",
        _ => "Keys"
      };
    }
  }
  
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  [PublicAPI] public string TemplateProgramName {
    get {
      if (TemplateSoundBankName == "Factory") {
        switch (Name) {
          case "Brutal Bass 2.1":
            return "808 Line";
          case "Lo-Fi 2.5" or "RetroWave 2.5" or "VCF-20 Synths 2.5":
            return "BAS Gameboy Bass";
          case "Organic Texture 2.8":
            return "BAS Biggy";
        }
      }
      return TemplateSoundBankName switch {
        "Hypnotic Drive" => "Lead - Acid Gravel",
        "Organic Keys" => "A Rhapsody",
        "Voklm" => "Breath Five",
        _ => "DX Mania"
      };
    }
  }
  
  public string TemplateProgramPath { get; private set; } = null!;
  public ScriptProcessor? TemplateScriptProcessor { get; private set; }
  private string? TemplateScriptProcessorName {
    get {
      if (TemplateSoundBankName == "Factory" &&
          Name == "Brutal Bass 2.1") {
        return "EventProcessor9";
      }
      if (IsInfoPageLayoutInScript) {
        return TemplateSoundBankName switch {
          "Hypnotic Drive" => "EventProcessor99",
          _ => "EventProcessor0"
        };
      }
      return null;
    }
  }
  
  [PublicAPI] public string TemplateSoundBankName { get; private set; } = null!;

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
      "*" + BatchConfig.ProgramExtension);
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

  private string GetTemplateProgramPath() {
    var templateProgramFile = new FileInfo(
      Path.Combine(
        BatchConfig.GetSoundBankFolder(TemplateSoundBankName).FullName,
        TemplateCategoryName, TemplateProgramName + BatchConfig.ProgramExtension));
    if (!templateProgramFile.Exists) {
      throw new ApplicationException(
        $"Cannot find template file '{templateProgramFile.FullName}'.");
    }
    return templateProgramFile.FullName;
  }

  private ScriptProcessor? GetTemplateScriptProcessor() {
    if (!IsInfoPageLayoutInScript) {
      return null;
    }
    using var reader = new StreamReader(TemplateProgramPath);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    return 
      (from scriptProcessor in root.Program.ScriptProcessors
        where scriptProcessor.Name == TemplateScriptProcessorName
        select scriptProcessor).FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find {TemplateScriptProcessorName} in file '{TemplateProgramPath}'.");
  }

  public void Initialise() {
    Folder = GetFolder(Name);
    TemplateSoundBankName = IsInfoPageLayoutInScript
      ? SoundBankFolder.Name
      : "Factory";
    TemplateProgramPath = GetTemplateProgramPath();
    TemplateScriptProcessor = GetTemplateScriptProcessor();
  }
}