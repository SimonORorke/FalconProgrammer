using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model.XmlLinq;

[TestFixture]
public class EmbeddedTemplateTests {
  [Test]
  public void Main() {
    const string embeddedFileName = "OrganicPads_DahdsrController.xml";
    var embeddedXml = new EmbeddedTemplate(embeddedFileName);
    Assert.That(embeddedXml.EmbeddedFileName, Is.EqualTo(embeddedFileName));
    Assert.That(embeddedXml.RootElement.Name.LocalName,
      Is.EqualTo("OrganicPads_DahdsrController"));
  }
}