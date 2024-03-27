using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

[XmlRoot(nameof(SettingsFolderLocation))]
public class SettingsFolderLocation : SerialisationBase {
  [XmlAttribute] public string Path { get; set; } = string.Empty;

  /// <summary>
  ///   Can be dispensed with once all settings can be specified via the GUI.
  /// </summary>
  /// <remarks>
  ///   C:\Users\Simon O'Rorke\AppData\Local\Packages\com.simonororke.falconprogrammer_9zz4h110yvjzm\LocalState
  /// </remarks>
  [ExcludeFromCodeCoverage]
  [SuppressMessage("ReSharper", "CommentTypo")]
  public static string? AppDataFolderPathMaui { get; set; }

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
    string appDataFolderPath = AppDataFolderPathMaui ?? System.IO.Path.Combine(
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