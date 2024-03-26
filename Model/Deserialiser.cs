using System.Xml.Serialization;

namespace FalconProgrammer.Model;

internal class Deserialiser<T> : SerialisationBase where T : SerialisationBase {

  public T Deserialise(Stream stream) {
    var deserializer = new XmlSerializer(typeof(T));
    var result = (T)deserializer.Deserialize(stream)!;
    PopulateUtilityProperties(result);
    return result;
  }

  public virtual T Deserialise(string inputPath) {
    T? result = null;
    if (FileSystemService.FileExists(inputPath)) {
      using var reader = new StreamReader(inputPath);
      var deserializer = new XmlSerializer(typeof(T));
      try {
        result = (T)deserializer.Deserialize(reader)!;
      } catch (Exception) { // XML error
      }
    }
    // If the XML file does not exist or contains an XML error,
    // return a new object with just the utility properties populated.
    result ??= (T)Activator.CreateInstance(typeof(T))!;
    PopulateUtilityProperties(result);
    return result;
  }

  private void PopulateUtilityProperties(T deserialisedObject) {
    deserialisedObject.ApplicationName = ApplicationName;
    deserialisedObject.FileSystemService = FileSystemService;
    deserialisedObject.Serialiser = Serialiser;
  }
}