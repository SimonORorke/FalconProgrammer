namespace FalconProgrammer.Model;

/// <summary>
///   A utility that can serialise an object to a file.
/// </summary>
public interface ISerialiser {
  void Serialise(object objectToSerialise, string outputPath);
}