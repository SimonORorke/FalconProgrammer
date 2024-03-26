using System.Text;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class DeserialiserTests {
  [Test]
  public void XmlError() {
    var deserialiser = new Deserialiser<SettingsFolderLocation>();
    using var invalidXmlStream = GenerateStreamFromString("This is not valid XML.");
    var settingsFolderLocation = deserialiser.Deserialise(invalidXmlStream);
    Assert.That(settingsFolderLocation.Path, Is.Empty);
  }

  private static MemoryStream GenerateStreamFromString(string value) {
    return new MemoryStream(Encoding.UTF8.GetBytes(value));
  }
}