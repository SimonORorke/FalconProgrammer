using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class FolderTests {
  [Test]
  public void FolderPathTrimsTrailingDirectorySeparator() {
    var folder = new Folder();
    Assert.That(folder.Path, Is.EqualTo(string.Empty));

#if OS_WINDOWS
    folder.Path = @"C:\Users\Simon\Documents\UVI\FalconProgrammer.Data\Programs\";
    Assert.That(folder.Path, Is.EqualTo(@"C:\Users\Simon\Documents\UVI\FalconProgrammer.Data\Programs"));

    folder.Path = @"C:/Users/Simon/Documents/UVI/FalconProgrammer.Data/Programs/";
    Assert.That(folder.Path, Is.EqualTo(@"C:/Users/Simon/Documents/UVI/FalconProgrammer.Data/Programs"));
#elif OS_MAC
    folder.Path = "/Users/Simon/Documents/UVI/FalconProgrammer.Data/Programs/";
    Assert.That(folder.Path, Is.EqualTo("/Users/Simon/Documents/UVI/FalconProgrammer.Data/Programs"));
#endif

    folder.Path = "   /some/path/   ";
    Assert.That(folder.Path, Is.EqualTo("/some/path"));

    folder.Path = "   ";
    Assert.That(folder.Path, Is.EqualTo(string.Empty));

    folder.Path = null!;
    Assert.That(folder.Path, Is.EqualTo(string.Empty));
  }

  [Test]
  public void SettingsFolderLocationPathTrimsTrailingDirectorySeparator() {
    var location = new SettingsFolderLocation();
    Assert.That(location.Path, Is.EqualTo(string.Empty));

#if OS_WINDOWS
    location.Path = @"C:\Users\Simon\AppData\Roaming\FalconProgrammer\";
    Assert.That(location.Path, Is.EqualTo(@"C:\Users\Simon\AppData\Roaming\FalconProgrammer"));
#elif OS_MAC
    location.Path = "/Users/Simon/Library/Application Support/FalconProgrammer/";
    Assert.That(location.Path, Is.EqualTo("/Users/Simon/Library/Application Support/FalconProgrammer"));
#endif

    location.Path = "   ";
    Assert.That(location.Path, Is.EqualTo(string.Empty));

    location.Path = null!;
    Assert.That(location.Path, Is.EqualTo(string.Empty));
  }
}
