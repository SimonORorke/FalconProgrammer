using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer;

[XmlRoot(nameof(Settings))]
public class Settings {
  [PublicAPI] public const string DefaultSettingsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\FalconProgrammer Settings";

  public Settings() {
    ProgramsFolder = new Folder();
    ProgramCategories = new List<ProgramCategory>();
  }

  [XmlElement] public Folder ProgramsFolder { get; set; }

  [XmlArray("ProgramCategories")]
  [XmlArrayItem(nameof(ProgramCategory))]
  public List<ProgramCategory> ProgramCategories { get; set; }
  // In test coverage, this counts as two commands, one of which is not covered.
  // So we initialise the list in the constructor.
  // public List<ProgramTemplate> ProgramTemplates { get; set; } =
  //   new List<ProgramTemplate>();

  [PublicAPI] [XmlIgnore] public string SettingsPath { get; set; } = string.Empty;

  internal static FileInfo GetSettingsFile(string settingsFolderPath) {
    return new FileInfo(Path.Combine(settingsFolderPath, "Settings.xml"));
  }
  
  public ProgramCategory GetProgramCategory(
    string soundBankFolderName, string categoryName) {
    var result = ((
      from programCategory in ProgramCategories
      where programCategory.SoundBank == soundBankFolderName &&
            programCategory.Category == categoryName
      select programCategory).FirstOrDefault() ?? (
      from programCategory in ProgramCategories
      where programCategory.SoundBank == soundBankFolderName &&
            programCategory.Category == string.Empty
      select programCategory).FirstOrDefault()) ?? (
      from programCategory in ProgramCategories
      where programCategory.SoundBank == string.Empty &&
            programCategory.Category == string.Empty
      select programCategory).First();
    return result;
  }

  public static Settings Read(
    string settingsFolderPath = DefaultSettingsFolderPath,
    string applicationName = SettingsFolderLocation.DefaultApplicationName) {
    var settingsFolderLocation = SettingsFolderLocation.Read(applicationName);
    if (string.IsNullOrEmpty(settingsFolderLocation.Path)) {
      settingsFolderLocation.Path = settingsFolderPath;
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
    [XmlAttribute] public bool IsInfoPageLayoutInScript { get; set; }
    [XmlAttribute] public string TemplatePath { get; set; } = string.Empty;
  }
}