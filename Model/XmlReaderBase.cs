namespace FalconProgrammer.Model;

public abstract class XmlReaderBase<T> : SerialisationBase where T : SerialisationBase {
  private Deserialiser<T>? _deserialiser;

  internal override string ApplicationName {
    get => base.ApplicationName;
    set => Deserialiser.ApplicationName = base.ApplicationName = value;
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