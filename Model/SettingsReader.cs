using FalconProgrammer.Model.Options;

namespace FalconProgrammer.Model;

public class SettingsReader : XmlReaderBase<Settings> {
  public virtual Settings Read(bool useDefaultIfNotFound = false) {
    var settingsFolderLocationReader = CreateSettingsFolderLocationReader();
    var settingsFolderLocation = settingsFolderLocationReader.Read();
    if (string.IsNullOrEmpty(settingsFolderLocation.Path)) {
      settingsFolderLocation.Path = GetDefaultSettingsFolderPath();
      if (!string.IsNullOrEmpty(settingsFolderLocation.Path)) {
        // Tests only
        settingsFolderLocation.Write();
      }
    }
    string settingsPath = Settings.GetSettingsPath(settingsFolderLocation.Path);
    var result =
      FileSystemService.File.Exists(settingsPath) || !useDefaultIfNotFound
        ? Deserialiser.Deserialise(settingsPath)
        : Deserialiser.Deserialise(Global.GetEmbeddedFileStream("DefaultSettings.xml"));
    result.SettingsPath = settingsPath;
    return result;
  }

  /// <summary>
  ///   The default is an empty string, indicating that the settings folder path has not
  ///   been specified, i.e. settings folder location file does not exist or the path
  ///   cannot be read from it.  Can be overridden for testing.
  /// </summary>
  protected virtual string GetDefaultSettingsFolderPath() {
    return string.Empty;
  }

  protected virtual SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return new SettingsFolderLocationReader {
      AppDataFolderName = AppDataFolderName,
      FileSystemService = FileSystemService,
      Serialiser = Serialiser
    };
  }
}