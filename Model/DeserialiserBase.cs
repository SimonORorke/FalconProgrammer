using System.Xml.Serialization;

namespace FalconProgrammer.Model;

public abstract class DeserialiserBase<T>(
  IFileSystemService fileSystemService,
  ISerialiser serialiser,
  string applicationName = Global.ApplicationName) where T : SerialisableBase {
  
  protected string ApplicationName { get; } = applicationName;
  protected IFileSystemService FileSystemService { get; } = fileSystemService;
  protected ISerialiser Serialiser { get; } = serialiser;

  protected T Deserialise(string inputPath) {
    T result;
    if (FileSystemService.FileExists(inputPath)) {
      using var reader = new StreamReader(inputPath);
      var deserializer = new XmlSerializer(typeof(T));
      result = (T)deserializer.Deserialize(reader)!;
    } else {
      result = (T)Activator.CreateInstance(typeof(T))!;
    }
    PopulateUtilityProperties(result);
    return result;
  }

  protected T Deserialise(Stream stream) {
    var deserializer = new XmlSerializer(typeof(T));
    var result = (T)deserializer.Deserialize(stream)!;
    PopulateUtilityProperties(result);
    return result;
  }

  private void PopulateUtilityProperties(T deserialisedObject) {
    deserialisedObject.ApplicationName = ApplicationName;
    deserialisedObject.FileSystemService = FileSystemService;
    deserialisedObject.Serialiser = Serialiser;
  }
}