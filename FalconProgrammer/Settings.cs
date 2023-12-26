using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer;

[XmlRoot(nameof(Settings))]
public class Settings {
  [PublicAPI] public const string DefaultSettingsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Settings";

  [XmlElement] public Folder BatchScriptsFolder { get; set; } = new Folder();
  [XmlElement] public Folder ProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder OriginalProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder TemplateProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Template DefaultTemplate { get; set; } = new Template();

  [XmlArray("MustUseGuiScriptProcessor")]
  [XmlArrayItem(nameof(ProgramCategory))]
  public List<ProgramCategory> MustUseGuiScriptProcessorCategories { get; set; } = [];

  [PublicAPI] [XmlIgnore] public string SettingsPath { get; set; } = string.Empty;

  internal static FileInfo GetSettingsFile(string settingsFolderPath) {
    return new FileInfo(Path.Combine(settingsFolderPath, "Settings.xml"));
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

  public static Settings Read(
    string defaultSettingsFolderPath = DefaultSettingsFolderPath,
    string applicationName = SettingsFolderLocation.DefaultApplicationName) {
    var settingsFolderLocation = SettingsFolderLocation.Read(applicationName);
    if (string.IsNullOrEmpty(settingsFolderLocation.Path)) {
      settingsFolderLocation.Path = defaultSettingsFolderPath;
      settingsFolderLocation.Write();
    }
    var settingsFile = GetSettingsFile(settingsFolderLocation.Path);
    if (!settingsFile.Exists) {
      return new Settings { SettingsPath = settingsFile.FullName };
    }
    using var reader = new StreamReader(settingsFile.FullName);
    var serializer = new XmlSerializer(typeof(Settings));
    var result = (Settings)serializer.Deserialize(reader)!;
    result.SettingsPath = settingsFile.FullName;
    return result;
  }

  public void Write() {
    var serializer = new XmlSerializer(typeof(Settings));
    using var writer = new StreamWriter(SettingsPath);
    serializer.Serialize(writer, this);
  }

  public class Folder {
    [XmlAttribute] public string Path { get; set; } = string.Empty;
  }

  public class ProgramCategory {
    [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
    [XmlAttribute] public string Category { get; set; } = string.Empty;
  }

  public class Template {
    [XmlAttribute] public string SubPath { get; set; } = string.Empty;
  }
}