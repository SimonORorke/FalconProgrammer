using System.Xml.Serialization;

namespace FalconProgrammer.Model;

internal abstract class DeserialiserBase(
  IFileSystemService fileSystemService,
  ISerialiser serialiser,
  string applicationName = Global.ApplicationName) {
  
  private string ApplicationName { get; } = applicationName;
  private IFileSystemService FileSystemService { get; } = fileSystemService;
  private ISerialiser Serialiser { get; } = serialiser;

  protected SerialisableBase Deserialise(string inputPath, Type type) {
    using var reader = new StreamReader(inputPath);
    var deserializer = new XmlSerializer(type);
    var result = (SerialisableBase)deserializer.Deserialize(reader)!;
    result.ApplicationName = ApplicationName;
    result.FileSystemService = FileSystemService;
    result.Serialiser = Serialiser;
    return result;
  }
}