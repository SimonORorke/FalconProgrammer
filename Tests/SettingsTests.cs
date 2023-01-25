namespace FalconProgrammer.Tests;

[TestFixture]
public class SettingsTests {
  [Test]
  public void Test1() {
    SettingsTestHelper.DeleteAnyData();
    try {
      var settings = SettingsTestHelper.ReadSettings();
      Assert.That(!File.Exists(settings.SettingsPath));
      Assert.IsEmpty(settings.ProgramCategories);
      SettingsTestHelper.WriteSettings(settings);
      settings = SettingsTestHelper.ReadSettings();
      Assert.That(File.Exists(settings.SettingsPath));
      Assert.That(settings.ProgramsFolder.Path, Is.EqualTo(
        SettingsTestHelper.ProgramsFolderPath));
      Assert.That(settings.ProgramCategories, Has.Count.EqualTo(17));
    } finally {
      SettingsTestHelper.DeleteAnyData();
    }
  }
}