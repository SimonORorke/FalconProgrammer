using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class SettingsFolderLocationTests {
  [Test]
  public void RealTestSettingsFolderLocationFile() {
    SettingsTestHelper.DeleteAnyData();
    try {
      var settingsFolderLocationReader = new SettingsFolderLocationReader {
        AppDataFolderName = SettingsTestHelper.TestAppDataFolderName
      };
      var location1 = settingsFolderLocationReader.Read();
      Assert.That(location1.Path, Is.Empty);
      location1.Path = SettingsTestHelper.TestSettingsFolderPath;
      location1.Write();
      Assert.That(Directory.Exists(SettingsTestHelper.TestSettingsFolderPath));
      var location2 = settingsFolderLocationReader.Read();
      Assert.That(location2.Path, Is.EqualTo(SettingsTestHelper.TestSettingsFolderPath));
    } finally {
      SettingsTestHelper.DeleteAnyData();
    }
  }
}