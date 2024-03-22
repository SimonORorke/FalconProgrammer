using System.Reflection;

namespace FalconProgrammer.Model;

public class SettingsReader : XmlReaderBase<Settings> {
  
  /// <summary>
  ///   Currently only used in test
  ///   LocationsViewModelTests.CancelBrowseForDefaultTemplate.
  ///   There are better ways.
  ///   TODO: Replace SettingsReader.DefaultSettingsFolderPath.
  /// </summary>
  public string DefaultSettingsFolderPath { get; set; } = 
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Settings";

  public virtual Settings Read(bool useDefaultIfNotFound = false) {
    var settingsFolderLocationReader = CreateSettingsFolderLocationReader();
    var settingsFolderLocation = settingsFolderLocationReader.Read();
    if (string.IsNullOrEmpty(settingsFolderLocation.Path)) {
      settingsFolderLocation.Path = DefaultSettingsFolderPath;
      settingsFolderLocation.Write();
    }
    string settingsPath = Settings.GetSettingsPath(settingsFolderLocation.Path);
    var result =
      FileSystemService.FileExists(settingsPath) || !useDefaultIfNotFound
        ? Deserialiser.Deserialise(settingsPath)
        : Deserialiser.Deserialise(GetDefaultSettingsStream());
    // result.ApplicationName = ApplicationName;
    // result.FileSystemService = FileSystemService;
    // result.Serialiser = Serialiser;
    result.SettingsPath = settingsPath;
    return result;
  }

  protected virtual SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return new SettingsFolderLocationReader {
      ApplicationName = ApplicationName,
      FileSystemService = FileSystemService,
      Serialiser = Serialiser
    };
  }

  private static Stream GetDefaultSettingsStream() {
    var assembly = Assembly.GetExecutingAssembly();
    string resourceName = assembly.GetManifestResourceNames()
      .Single(resourcePath => resourcePath.EndsWith("DefaultSettings.xml"));
    return assembly.GetManifestResourceStream(resourceName)!;
  }
}