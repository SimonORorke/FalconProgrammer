using System.Xml.Serialization;

namespace FalconProgrammer.Model;

public abstract class SerialisationBase {
  internal string AppDataFolderName { get; set; } = Global.ApplicationName;

  /// <summary>
  ///   A utility that can access the file system. The default is a real
  ///   <see cref="FileSystemService" />. Can be set to a mock for testing.
  /// </summary>
  [XmlIgnore]
  public IFileSystemService FileSystemService {
    get => field ??= Model.FileSystemService.Default;
    set;
  }

  /// <summary>
  ///   A utility that can serialise an object to a file. The default is a real
  ///   <see cref="Serialiser" />. Can be set to a mock serialiser for testing.
  /// </summary>
  [XmlIgnore]
  public ISerialiser Serialiser {
    get => field ??= Model.Serialiser.Default;
    set;
  }
}