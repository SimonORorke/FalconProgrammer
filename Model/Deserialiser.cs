using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

public class Deserialiser<T> : SerialisationBase where T : SerialisationBase {
  public T Deserialise(Stream? stream) {
    var deserializer = new XmlSerializer(typeof(T));
    T? result = null;
    if (stream != null) {
      try {
        result = (T)deserializer.Deserialize(stream)!;
      } catch (Exception) { // XML error
        throw new XmlException();
      }
    }
    // If the XML file does not exist or contains an XML error,
    // return a new object with just the utility properties populated.
    result ??= (T)Activator.CreateInstance(typeof(T))!;
    PopulateUtilityProperties(result);
    return result;
  }

  public virtual T Deserialise(string inputPath) {
    using var fileStream = FileSystemService.File.Exists(inputPath)
      ? File.OpenRead(inputPath)
      : null;
    return DeserialiseFileStream(inputPath, fileStream);
  }

  [ExcludeFromCodeCoverage]
  private T DeserialiseFileStream(string inputPath, Stream? fileStream) {
    try {
      return Deserialise(fileStream);
    } catch (XmlException) {
      throw new XmlException($"Invalid XML was found in '{inputPath}'.");
    }
  }

  private void PopulateUtilityProperties(T deserialisedObject) {
    deserialisedObject.AppDataFolderName = AppDataFolderName;
    deserialisedObject.FileSystemService = FileSystemService;
    deserialisedObject.Serialiser = Serialiser;
  }
}