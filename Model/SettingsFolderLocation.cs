using System.Xml.Serialization;

namespace FalconProgrammer.Model;

[XmlRoot(nameof(SettingsFolderLocation))]
public class SettingsFolderLocation : SerialisationBase, ISettingsFolderLocation {
  [XmlAttribute] public string Path { get; set; } = string.Empty;

  public void Write() {
    string appDataFolderPath = GetAppDataFolderPath(AppDataFolderName);
    if (!FileSystemService.Folder.Exists(appDataFolderPath)) {
      FileSystemService.Folder.Create(appDataFolderPath);
    }
    Serialiser.Serialise(this,
      GetSettingsFolderLocationPath(AppDataFolderName));
    if (!string.IsNullOrWhiteSpace(Path)) {
      FileSystemService.Folder.Create(Path);
    }
  }

  internal static string GetAppDataFolderPath(
    string? appDataFolderName = null) {
    string appDataFolderPath = System.IO.Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      appDataFolderName ?? Global.ApplicationName);
    return appDataFolderPath;
  }

  internal static string GetSettingsFolderLocationPath(
    string? appDataFolderName = null) {
    return System.IO.Path.Combine(GetAppDataFolderPath(appDataFolderName),
      "SettingsFolderLocation.xml");
  }
}