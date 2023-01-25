namespace FalconProgrammer.Tests;

public class SettingsFolderLocationTests {
  [Test]
  public void Test1() {
    SettingsTestHelper.DeleteAnyData();
    try {
      var location1 = SettingsFolderLocation.Read(SettingsTestHelper.TestApplicationName);
      Assert.That(location1.Path, Is.Empty);
      location1.Path = SettingsTestHelper.SettingsFolderPath;
      location1.Write(SettingsTestHelper.TestApplicationName);
      Assert.That(Directory.Exists(SettingsTestHelper.SettingsFolderPath));
      var location2 = SettingsFolderLocation.Read(SettingsTestHelper.TestApplicationName);
      Assert.That(location2.Path, Is.EqualTo(SettingsTestHelper.SettingsFolderPath));
    } finally {
      SettingsTestHelper.DeleteAnyData();
    }
  }
}