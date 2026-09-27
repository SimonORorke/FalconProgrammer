namespace FalconProgrammer.Model;

public abstract class XmlReaderBase<T> : SerialisationBase where T : SerialisationBase {
  internal Deserialiser<T> Deserialiser {
    get => field ??= new Deserialiser<T>();
    // For tests.
    set;
  }
}