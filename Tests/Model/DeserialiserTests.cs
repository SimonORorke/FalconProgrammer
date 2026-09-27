using System.Text;
using System.Xml;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class DeserialiserTests {
  [Test]
  public void XmlErrorInStream() {
    var deserialiser = new Deserialiser<SettingsFolderLocation>();
    using var invalidXmlStream = GenerateStreamFromString("This is not valid XML.");
    Assert.Throws<XmlException>(() => deserialiser.Deserialise(invalidXmlStream));
  }

  private static MemoryStream GenerateStreamFromString(string value) {
    return new MemoryStream(Encoding.UTF8.GetBytes(value));
  }
}