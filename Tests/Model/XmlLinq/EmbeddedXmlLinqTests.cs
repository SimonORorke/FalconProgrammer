using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model.XmlLinq;

[TestFixture]
public class EmbeddedXmlLinqTests {
  [Test]
  public void Main() {
    const string embeddedFileName = "OrganicPads_DahdsrController.xml";
    var embeddedXmlLinq = new EmbeddedXmlLinq(embeddedFileName);
    Assert.That(embeddedXmlLinq.EmbeddedFileName, Is.EqualTo(embeddedFileName));
    Assert.That(embeddedXmlLinq.RootElement.Name.LocalName, 
      Is.EqualTo("EventProcessors"));
  }
}