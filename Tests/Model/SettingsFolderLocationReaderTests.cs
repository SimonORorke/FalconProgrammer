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
      EmbeddedFileName = "SettingsFolderLocation.xml"
    };
    Assert.DoesNotThrow(() => reader.Read());
  }

  [Test]
  public void XmlError() {
    var reader = new TestSettingsFolderLocationReader {
      FileSystemService = new MockFileSystemService(),
      EmbeddedFileName = "InvalidXmlSettingsFolderLocation.xml"
    };
    Assert.DoesNotThrow(() => reader.Read());
  }
}