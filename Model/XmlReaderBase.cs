namespace FalconProgrammer.Model;

public abstract class XmlReaderBase<T> : SerialisationBase where T : SerialisationBase {
  private Deserialiser<T>? _deserialiser;

  internal override string AppDataFolderName {
    get => base.AppDataFolderName;
    set => Deserialiser.AppDataFolderName = base.AppDataFolderName = value;
  }

  public override IFileSystemService FileSystemService {
    get => base.FileSystemService;
    set => Deserialiser.FileSystemService = base.FileSystemService = value;
  }

  public override ISerialiser Serialiser {
    get => base.Serialiser;
    set => Deserialiser.Serialiser = base.Serialiser = value;
  }

  internal virtual Deserialiser<T> Deserialiser {
    get => _deserialiser ??= new Deserialiser<T>();
    // For tests.
    set => _deserialiser = value;
  }
}