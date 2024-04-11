﻿using System.Xml.Serialization;

namespace FalconProgrammer.Model;

public abstract class SerialisationBase {
  private IFileSystemService? _fileSystemService;
  private ISerialiser? _serialiser;
  internal virtual string AppDataFolderName { get; set; } = Global.AppDataFolderName;

  /// <summary>
  ///   A utility that can access the file system. The default is a real
  ///   <see cref="FileSystemService" />. Can be set to a mock for testing.
  /// </summary>
  [XmlIgnore]
  public virtual IFileSystemService FileSystemService {
    get => _fileSystemService ??= Model.FileSystemService.Default;
    set => _fileSystemService = value;
  }

  /// <summary>
  ///   A utility that can serialise an object to a file. The default is a real
  ///   <see cref="Serialiser" />. Can be set to a mock serialiser for testing.
  /// </summary>
  [XmlIgnore]
  public virtual ISerialiser Serialiser {
    get => _serialiser ??= Model.Serialiser.Default;
    set => _serialiser = value;
  }
}