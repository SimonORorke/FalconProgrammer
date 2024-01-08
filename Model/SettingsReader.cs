namespace FalconProgrammer.Model;

public class SettingsReader(
  IFileSystemService fileSystemService,
  ISerialiser serialiser,
  string applicationName = Global.ApplicationName)
  : DeserialiserBase<Settings>(
    fileSystemService, serialiser, applicationName) {
  public string DefaultSettingsFolderPath { get; set; } = string.Empty;

  public Settings Read() {
    var settingsFolderLocationReader = new SettingsFolderLocationReader(
      FileSystemService, Serialiser, ApplicationName);
    var settingsFolderLocation = settingsFolderLocationReader.Read();
    if (string.IsNullOrEmpty(settingsFolderLocation.Path)) {
      settingsFolderLocation.Path = DefaultSettingsFolderPath;
      settingsFolderLocation.Write();
    }
    string settingsPath = Settings.GetSettingsPath(settingsFolderLocation.Path);
    var result = Deserialise(settingsPath);
    result.SettingsPath = settingsPath;
    return result;
  }
}