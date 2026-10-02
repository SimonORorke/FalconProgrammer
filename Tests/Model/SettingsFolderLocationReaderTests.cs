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
#if OS_WINDOWS
      EmbeddedFileName = "SettingsFolderLocation.xml"
#elif OS_MAC
      EmbeddedFileName = "SettingsFolderLocationMac.xml"
#endif
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