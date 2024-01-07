namespace FalconProgrammer.Model;

/// <summary>
///   A utility that can serialize an object to a file. 
/// </summary>
public interface ISerializer {
  void Serialize(Type type, object objectToSerialise, string outputPath);
}