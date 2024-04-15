namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class SettingsFolderLocationReaderTests {
  [Test]
  public void CannotCreateSettingsFolder() {
    var mockFileSystemService = new MockFileSystemService {
      Folder = {
        CanCreate = false
      }
    };
    var reader = new TestSettingsFolderLocationReader {
      FileSystemService = mockFileSystemService,
      TestDeserialiser = {
        EmbeddedResourceFileName = "SettingsFolderLocation.xml"
      }
    };
    Assert.DoesNotThrow(() => reader.Read());
  }

  [Test]
  public void XmlError() {
    var reader = new TestSettingsFolderLocationReader {
      FileSystemService = new MockFileSystemService(),
      TestDeserialiser = {
        EmbeddedResourceFileName = "InvalidXmlSettingsFolderLocation.xml"
      }
    };
    Assert.DoesNotThrow(() => reader.Read());
  }
}