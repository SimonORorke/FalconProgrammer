using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace FalconProgrammer.Model; 

[XmlRoot(nameof(SettingsFolderLocation))] public class SettingsFolderLocation {
  public const string DefaultApplicationName = "FalconProgrammer"; 
  
  private IFileSystemService? _fileSystemService;
  private ISerializer? _serializer;
  [XmlAttribute] public string Path { get; set; } = string.Empty;

  /// <summary>
  ///   A utility that can serialize an object to a file. The default is a real
  ///   <see cref="FileSystemService" />. Can be set to a mock for testing. 
  /// </summary>
  [XmlIgnore] public IFileSystemService FileSystemService {
    get => _fileSystemService ??= Model.FileSystemService.Default;
    set => _fileSystemService = value;
  }

  /// <summary>
  ///   A utility that can serialize an object to a file. The default is a real
  ///   <see cref="Serializer" />. Can be set to a mock serializer for testing. 
  /// </summary>
  [XmlIgnore] public ISerializer Serializer {
    get => _serializer ??= Model.Serializer.Default;
    set => _serializer = value;
  }

  public static SettingsFolderLocation Read(
    IFileSystemService fileSystemService,
    ISerializer writeSerializer,
    string applicationName = DefaultApplicationName) {
    var locationFile = GetSettingsFolderLocationFile(applicationName);
    SettingsFolderLocation result;
    if (locationFile.Exists) {
      using var reader = new StreamReader(locationFile.FullName);
      var readSerializer = new XmlSerializer(typeof(SettingsFolderLocation));
      result = (SettingsFolderLocation)readSerializer.Deserialize(reader)!;
      if (!string.IsNullOrWhiteSpace(result.Path)) {
        fileSystemService.CreateFolder(result.Path);
      }
    } else {
      result = new SettingsFolderLocation();
    }
    result.FileSystemService = fileSystemService;
    result.Serializer = writeSerializer;
    return result;
  }

  public void Write(
    string applicationName = DefaultApplicationName) {
    string appDataFolderPath = GetAppDataFolderPath(applicationName);
    if (!FileSystemService.FolderExists(appDataFolderPath)) {
      FileSystemService.CreateFolder(appDataFolderPath);
    }
    var locationFile = GetSettingsFolderLocationFile(applicationName);
    Serializer.Serialize(
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
    string applicationName = DefaultApplicationName) {
    string appDataFolderPath = AppDataFolderPathMaui ?? System.IO.Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      applicationName); 
    return appDataFolderPath;
  }

  internal static FileInfo GetSettingsFolderLocationFile(
    string applicationName = DefaultApplicationName) {
    return new FileInfo(
      System.IO.Path.Combine(GetAppDataFolderPath(applicationName), 
        "SettingsFolderLocation.xml"));
  }
}