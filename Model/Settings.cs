using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

/// <summary>
///   To read the settings, use <see cref="SettingsReader" />.
/// </summary>
[XmlRoot(nameof(Settings))]
public class Settings : SerialisationBase {
  [XmlElement] public Folder BatchScriptsFolder { get; set; } = new Folder();
  [XmlElement] public Folder ProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder OriginalProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder TemplateProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Template DefaultTemplate { get; set; } = new Template();

  [XmlArray("MustUseGuiScriptProcessor")]
  [XmlArrayItem(nameof(ProgramCategory))]
  [ExcludeFromCodeCoverage]
  public List<ProgramCategory> MustUseGuiScriptProcessorCategories { get; set; } = [];

  [XmlElement] public MidiForMacros MidiForMacros { get; set; } = new MidiForMacros();
  [XmlIgnore] public string SettingsPath { get; set; } = string.Empty;

  internal static string GetSettingsPath(string settingsFolderPath) {
    return !string.IsNullOrWhiteSpace(settingsFolderPath)
      ? Path.Combine(settingsFolderPath, "Settings.xml")
      : string.Empty;
  }

  public bool MustUseGuiScriptProcessor(
    string soundBankFolderName, string categoryName) {
    bool result = (
      from programCategory in MustUseGuiScriptProcessorCategories
      where programCategory.SoundBank == soundBankFolderName &&
            programCategory.Category == categoryName
      select programCategory).Any();
    if (!result) {
      result = (
        from programCategory in MustUseGuiScriptProcessorCategories
        where programCategory.SoundBank == soundBankFolderName &&
              programCategory.Category == string.Empty
        select programCategory).Any();
    }
    return result;
  }

  public void Write(string? settingsFolderPath = null) {
    if (settingsFolderPath != null) {
      SettingsPath = GetSettingsPath(settingsFolderPath);
    }
    Serialiser.Serialise(typeof(Settings), this, SettingsPath);
  }

  public class Folder {
    [XmlAttribute] public string Path { get; set; } = string.Empty;
  }

  public struct IntegerRange {
    public int Start;
    public int End;
  }

  public class ProgramCategory {
    [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
    [XmlAttribute] public string Category { get; set; } = string.Empty;
  }

  public class Template {
    [XmlAttribute] public string Path { get; set; } = string.Empty;
  }
}