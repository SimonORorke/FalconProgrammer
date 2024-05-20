using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class MockSerialiser : ISerialiser {
  internal Type LastType { get; set; } = null!;
  internal object LastObjectSerialised { get; set; } = null!;
  internal string LastOutputPath { get; set; } = string.Empty;
  internal string LastOutputText { get; set; } = string.Empty;
  internal int SerializeCount { get; set; }

  public void Serialise(Type type, object objectToSerialise, string outputPath) {
    LastType = type;
    LastObjectSerialised = objectToSerialise;
    LastOutputPath = outputPath;
    SerializeCount++;
    var serializer = new XmlSerializer(objectToSerialise.GetType());
    using var writer = new StringWriter();
    serializer.Serialize(writer, objectToSerialise);
    LastOutputText = writer.ToString();
  }
}