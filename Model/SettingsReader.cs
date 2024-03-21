using System.Reflection;

namespace FalconProgrammer.Model;

public class SettingsReader : DeserialiserBase<Settings> {
  
  /// <summary>
  ///   Currently only used in test
  ///   LocationsViewModelTests.CancelBrowseForDefaultTemplate.
  ///   There are better ways.
  ///   TODO: Replace SettingsReader.DefaultSettingsFolderPath.
  /// </summary>
  public string DefaultSettingsFolderPath { get; set; } = 
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Settings";

  public Settings Read(bool useDefaultIfNotFound = false) {
    var settingsFolderLocationReader = new SettingsFolderLocationReader {
      ApplicationName = ApplicationName,
      FileSystemService = FileSystemService,
      Serialiser = Serialiser
    };
    var settingsFolderLocation = settingsFolderLocationReader.Read();
    if (string.IsNullOrEmpty(settingsFolderLocation.Path)) {
      settingsFolderLocation.Path = DefaultSettingsFolderPath;
      settingsFolderLocation.Write();
    }
    string settingsPath = Settings.GetSettingsPath(settingsFolderLocation.Path);
    var result =
      FileSystemService.FileExists(settingsPath) || !useDefaultIfNotFound
        ? Deserialise(settingsPath)
        : Deserialise(GetDefaultSettingsStream());
    result.SettingsPath = settingsPath;
    return result;
  }

  private static Stream GetDefaultSettingsStream() {
    var assembly = Assembly.GetExecutingAssembly();
    string resourceName = assembly.GetManifestResourceNames()
      .Single(resourcePath => resourcePath.EndsWith("DefaultSettings.xml"));
    return assembly.GetManifestResourceStream(resourceName)!;
  }
}