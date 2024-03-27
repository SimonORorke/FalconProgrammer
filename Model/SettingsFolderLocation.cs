using System.Xml.Serialization;

namespace FalconProgrammer.Model;

[XmlRoot(nameof(SettingsFolderLocation))]
public class SettingsFolderLocation : SerialisationBase {
  [XmlAttribute] public string Path { get; set; } = string.Empty;

  public void Write() {
    string appDataFolderPath = GetAppDataFolderPath(ApplicationName);
    if (!FileSystemService.FolderExists(appDataFolderPath)) {
      FileSystemService.CreateFolder(appDataFolderPath);
    }
    Serialiser.Serialise(
      typeof(SettingsFolderLocation), this,
      GetSettingsFolderLocationPath(ApplicationName));
    if (!string.IsNullOrWhiteSpace(Path)) {
      FileSystemService.CreateFolder(Path);
    }
  }

  internal static string GetAppDataFolderPath(
    string applicationName = Global.ApplicationName) {
    string appDataFolderPath = System.IO.Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      applicationName);
    return appDataFolderPath;
  }

  internal static string GetSettingsFolderLocationPath(
    string applicationName = Global.ApplicationName) {
    return System.IO.Path.Combine(GetAppDataFolderPath(applicationName),
      "SettingsFolderLocation.xml");
  }
}