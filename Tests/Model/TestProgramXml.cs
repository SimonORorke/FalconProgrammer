using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

public class TestProgramXml : ProgramXml {
  public TestProgramXml(Category category) : base(category) { }

  /// <summary>
  ///   <see cref="ReadRootElementFromFile" /> will read the embedded resource file
  ///   with this name in the Tests assembly, ignoring its programPath parameter.
  /// </summary>
  internal string EmbeddedProgramFileName { get; set; } = "NoGuiScriptProcessor.uvip";

  private MemoryStream OutputStream { get; set; }
  internal string SavedXml { get; private set; } = string.Empty;

  private Stream GetEmbeddedProgramStream() {
    var assembly = Assembly.GetExecutingAssembly();
    string resourceName = XmlTestHelper.GetEmbeddedResourceName(
      EmbeddedProgramFileName, assembly);
    return assembly.GetManifestResourceStream(resourceName)!;
  }

  protected override void CloseXmlWriter(XmlWriter writer) {
    base.CloseXmlWriter(writer);
    OutputStream.Position = 0;
    using var reader = new StreamReader(OutputStream);
    SavedXml = reader.ReadToEnd();
  }

  protected override Stream CreateOutputStream(string outputProgramPath) {
    return OutputStream = new MemoryStream();
  }

  /// <summary>
  ///   The file specified by <paramref name="programPath" /> will not be accessed or
  ///   deserialised. Instead, the embedded resource file specified by
  ///   <see cref="EmbeddedProgramFileName" /> will be read from the Tests assembly and
  ///   deserialised.
  /// </summary>
  protected override XElement ReadRootElementFromFile(string programPath) {
    return ReadRootElementFromStream(GetEmbeddedProgramStream());
  }
}