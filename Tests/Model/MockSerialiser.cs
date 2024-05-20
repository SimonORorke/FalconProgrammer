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

  public void Serialise(object objectToSerialise, string outputPath) {
    LastObjectSerialised = objectToSerialise;
    LastOutputPath = outputPath;
    SerializeCount++;
    LastType = objectToSerialise.GetType();
    var serializer = new XmlSerializer(LastType);
    using var writer = new StringWriter();
    serializer.Serialize(writer, objectToSerialise);
    LastOutputText = writer.ToString();
  }
}