using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model.XmlLinq;

[TestFixture]
public class EmbeddedXmlTests {
  [Test]
  public void Main() {
    const string embeddedFileName = "OrganicPads_DahdsrController.xml";
    var embeddedXml = new EmbeddedXml(embeddedFileName);
    Assert.That(embeddedXml.EmbeddedFileName, Is.EqualTo(embeddedFileName));
    Assert.That(embeddedXml.RootElement.Name.LocalName,
      Is.EqualTo("OrganicPads_DahdsrController"));
  }
}