using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer; 

[XmlRoot(nameof(Settings))] public class Settings {
  [PublicAPI] public const string DefaultSettingsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\FalconProgrammer Settings";

  [XmlArray("ProgramTemplates")]
  [XmlArrayItem(nameof(ProgramTemplate))]
  public List<ProgramTemplate> ProgramTemplates { get; set; } = 
    new List<ProgramTemplate>();
  
  public class ProgramTemplate {
    [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
    [XmlAttribute] public string Category { get; set; } = string.Empty;
    [XmlAttribute] public string Path { get; set; } = string.Empty;
  }

  [PublicAPI] public string SettingsPath { get; private set; } = string.Empty;

  internal static FileInfo GetSettingsFile(string settingsFolderPath) {
    return new FileInfo(Path.Combine(settingsFolderPath, "Settings.xml"));
  }

  public static Settings Read(
    string applicationName = SettingsFolderLocation.DefaultApplicationName) {
    var settingsFolderLocation = SettingsFolderLocation.Read(applicationName);
    if (string.IsNullOrEmpty(settingsFolderLocation.Path)) {
      settingsFolderLocation.Path = DefaultSettingsFolderPath;
      settingsFolderLocation.Write();
    }
    var settingsFile = GetSettingsFile(settingsFolderLocation.Path);
    if (!settingsFile.Exists) {
      return new Settings();
    }
    var reader = new StreamReader(settingsFile.FullName);
    var serializer = new XmlSerializer(typeof(Settings));
    var result = (Settings)serializer.Deserialize(reader)!;
    result.SettingsPath = settingsFile.FullName;
    return result;
  }

  public void Write() {
    var serializer = new XmlSerializer(typeof(Settings));
     var writer = new StreamWriter(SettingsPath);
     serializer.Serialize(writer, this);
  }
}