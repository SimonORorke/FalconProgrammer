namespace FalconProgrammer.Model;

/// <summary>
///   A utility that can serialise an object to a file.
/// </summary>
public interface ISerialiser {
  // There's not point in making Serialise generic, as the Type is only used to pass
  // directly to XmlSerializer.
  void Serialise(Type type, object objectToSerialise, string outputPath);
}