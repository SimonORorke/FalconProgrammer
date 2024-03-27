using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class MockSerialiser : ISerialiser {
  internal Type LastType { get; set; } = null!;
  internal object LastObjectSerialised { get; set; } = null!;
  internal string LastOutputPath { get; set; } = string.Empty;
  internal int SerializeCount { get; set; }

  public void Serialise(Type type, object objectToSerialise, string outputPath) {
    LastType = type;
    LastObjectSerialised = objectToSerialise;
    LastOutputPath = outputPath;
    SerializeCount++;
  }
}