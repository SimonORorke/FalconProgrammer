using System.Xml.Serialization;

namespace FalconProgrammer.Model;

/// <summary>
///   A utility that can serialise an object to a file.
/// </summary>
public class Serialiser : ISerialiser {
  private static ISerialiser? _default;
  private Serialiser() { }
  public static ISerialiser Default => _default ??= new Serialiser();

  public void Serialise(Type type, object objectToSerialise, string outputPath) {
    var serializer = new XmlSerializer(type);
    using var writer = new StreamWriter(outputPath);
    try {
      serializer.Serialize(writer, objectToSerialise);
    } catch (IOException exception) {
      // Message =
      // 'The process cannot access the file ... because it is being used by another process.'
      // This can be a due to a lack of 'using'. But that is included above.
      // I can't replicate the problem by saving on one page and the another.
      Console.WriteLine($"Serialiser.Serialise: serializer.Serialize throwing IOException '{exception.Message}'");
      throw;
    }
  }
}