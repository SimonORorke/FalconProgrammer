namespace FalconProgrammer.Model;

public abstract class XmlReaderBase<T> : SerialisationBase where T : SerialisationBase {
  private Deserialiser<T>? _deserialiser;

  internal Deserialiser<T> Deserialiser {
    get => _deserialiser ??= new Deserialiser<T>();
    // For tests.
    set => _deserialiser = value;
  }
}