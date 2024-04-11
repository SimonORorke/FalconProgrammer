using System.Xml.Serialization;

namespace FalconProgrammer.Model;

internal class Deserialiser<T> : SerialisationBase where T : SerialisationBase {
  public T Deserialise(Stream? stream) {
    var deserializer = new XmlSerializer(typeof(T));
    T? result = null;
    if (stream != null) {
      try {
        result = (T)deserializer.Deserialize(stream)!;
      } catch (Exception) { // XML error
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
    return Deserialise(fileStream);
  }

  private void PopulateUtilityProperties(T deserialisedObject) {
    deserialisedObject.AppDataFolderName = AppDataFolderName;
    deserialisedObject.FileSystemService = FileSystemService;
    deserialisedObject.Serialiser = Serialiser;
  }
}