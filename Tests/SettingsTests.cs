namespace FalconProgrammer.Tests;

[TestFixture]
public class SettingsTests {
  [Test]
  public void Test1() {
    var settings = Settings.Read(SettingsFolderLocationTests.SettingsFolderPath,
      SettingsFolderLocationTests.TestApplicationName);
    Assert.IsEmpty(settings.SettingsPath);
    Assert.AreEqual(0, settings.ProgramTemplates.Count);
    DeleteAnyTestData(string.Empty);
    try {
    } finally {
      
    }
    // var settingsFile = new FileInfo(settings.SettingsPath);
    // Assert.AreEqual(SettingsFolderLocationTests.SettingsFolderPath,
    //   settingsFile.DirectoryName);
    // Assert.IsFalse(settingsFile.Exists);
  }

  private static void DeleteAnyTestData(string settingsPath) {
    if (!string.IsNullOrEmpty(settingsPath)) {
      var settingFile = new FileInfo(settingsPath);
      if (settingFile.Exists) {
        settingFile.Delete();
      }
    }
    SettingsFolderLocationTests.DeleteAnyTestData();
  }
}