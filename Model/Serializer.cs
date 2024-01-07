using System.Xml.Serialization;

namespace FalconProgrammer.Model;

/// <summary>
///   A utility that can serialize an object to a file.
/// </summary>
public class Serializer : ISerializer {
  private static ISerializer? _default;
  private Serializer() { }
  public static ISerializer Default => _default ??= new Serializer();

  public void Serialize(Type type, object objectToSerialise, string outputPath) {
    var serializer = new XmlSerializer(type);
    using var writer = new StreamWriter(outputPath);
    serializer.Serialize(writer, objectToSerialise);
  }
}