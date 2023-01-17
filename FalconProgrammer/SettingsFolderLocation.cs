using System.Xml.Serialization;

namespace FalconProgrammer; 

[XmlRoot(nameof(SettingsFolderLocation))] public class SettingsFolderLocation {
  public const string DefaultApplicationName = "FalconProgrammer"; 
  
  [XmlAttribute] public string Path { get; set; } = string.Empty;

  public static SettingsFolderLocation Read(
    string applicationName = DefaultApplicationName) {
    var locationFile = GetSettingsFolderLocationFile(applicationName);
    if (!locationFile.Exists) {
      return new SettingsFolderLocation();
    }
    using var reader = new StreamReader(locationFile.FullName);
    var serializer = new XmlSerializer(typeof(SettingsFolderLocation));
    var result = (SettingsFolderLocation)serializer.Deserialize(reader)!;
    Directory.CreateDirectory(result.Path);
    return result;
  }

  public void Write(
    string applicationName = DefaultApplicationName) {
    var appDataFolder = GetAppDataFolder(applicationName);
    if (!appDataFolder.Exists) {
      appDataFolder.Create();
    }
    var locationFile = GetSettingsFolderLocationFile(applicationName);
    var serializer = new XmlSerializer(typeof(SettingsFolderLocation));
    using var writer = new StreamWriter(locationFile.FullName);
    serializer.Serialize(writer, this);
    Directory.CreateDirectory(Path);
  }

  internal static DirectoryInfo GetAppDataFolder(
    string applicationName = DefaultApplicationName) {
    var result = new DirectoryInfo(System.IO.Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
      applicationName));
    return result;
  }

  internal static FileInfo GetSettingsFolderLocationFile(
    string applicationName = DefaultApplicationName) {
    return new FileInfo(System.IO.Path.Combine(
      GetAppDataFolder(applicationName).FullName, "SettingsFolderLocation.xml"));
  }
}