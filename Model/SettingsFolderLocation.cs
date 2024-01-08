using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace FalconProgrammer.Model; 

[XmlRoot(nameof(SettingsFolderLocation))] 
public class SettingsFolderLocation : SerialisableBase {
  [XmlAttribute] public string Path { get; set; } = string.Empty;

  public static SettingsFolderLocation Read(
    IFileSystemService fileSystemService,
    ISerialiser writeSerialiser,
    string applicationName = Global.ApplicationName) {
    var locationFile = GetSettingsFolderLocationFile(applicationName);
    SettingsFolderLocation result;
    if (locationFile.Exists) {
      using var reader = new StreamReader(locationFile.FullName);
      var deserializer = new XmlSerializer(typeof(SettingsFolderLocation));
      result = (SettingsFolderLocation)deserializer.Deserialize(reader)!;
      if (!string.IsNullOrWhiteSpace(result.Path)) {
        fileSystemService.CreateFolder(result.Path);
      }
    } else {
      result = new SettingsFolderLocation();
    }
    result.FileSystemService = fileSystemService;
    result.Serialiser = writeSerialiser;
    return result;
  }

  public void Write(
    string applicationName = Global.ApplicationName) {
    string appDataFolderPath = GetAppDataFolderPath(applicationName);
    if (!FileSystemService.FolderExists(appDataFolderPath)) {
      FileSystemService.CreateFolder(appDataFolderPath);
    }
    var locationFile = GetSettingsFolderLocationFile(applicationName);
    Serialiser.Serialise(
      typeof(SettingsFolderLocation), this, locationFile.FullName);
    if (!string.IsNullOrWhiteSpace(Path)) {
      FileSystemService.CreateFolder(Path);
    }
  }

  /// <summary>
  ///   Can be dispensed with once all settings can be specified via the GUI.
  /// </summary>
  /// <remarks>
  ///   C:\Users\Simon O'Rorke\AppData\Local\Packages\com.simonororke.falconprogrammer_9zz4h110yvjzm\LocalState
  /// </remarks>
  [SuppressMessage("ReSharper", "CommentTypo")]
  public static string? AppDataFolderPathMaui { get; set; }

  internal static string GetAppDataFolderPath(
    string applicationName = Global.ApplicationName) {
    string appDataFolderPath = AppDataFolderPathMaui ?? System.IO.Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      applicationName); 
    return appDataFolderPath;
  }

  internal static FileInfo GetSettingsFolderLocationFile(
    string applicationName = Global.ApplicationName) {
    return new FileInfo(
      System.IO.Path.Combine(GetAppDataFolderPath(applicationName), 
        "SettingsFolderLocation.xml"));
  }
}