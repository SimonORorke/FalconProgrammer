namespace FalconProgrammer.Tests;

[TestFixture]
public class SettingsTests {
  [Test]
  public void Test1() {
    SettingsTestHelper.DeleteAnyData();
    try {
      var settings = SettingsTestHelper.ReadSettings();
      Assert.That(!File.Exists(settings.SettingsPath));
      SettingsTestHelper.WriteSettings(settings);
      settings = SettingsTestHelper.ReadSettings();
      Assert.That(File.Exists(settings.SettingsPath));
      Assert.That(settings.BatchScriptsFolder.Path, Is.EqualTo(
        SettingsTestHelper.BatchScriptsFolderPath));
      Assert.That(settings.ProgramsFolder.Path, Is.EqualTo(
        SettingsTestHelper.ProgramsFolderPath));
      Assert.That(settings.OriginalProgramsFolder.Path, Is.EqualTo(
        SettingsTestHelper.OriginalProgramsFolderPath));
      Assert.That(settings.TemplateProgramsFolder.Path, Is.EqualTo(
        SettingsTestHelper.TemplateProgramsFolderPath));
      Assert.That(settings.DefaultTemplate.SubPath, Is.EqualTo(
        SettingsTestHelper.DefaultTemplateSubPath));
      Assert.That(settings.MustUseGuiScriptProcessorCategories, Has.Count.EqualTo(4));
      Assert.That(!settings.MustUseGuiScriptProcessor(
        "Factory", "Bass-Sub"));
      Assert.That(settings.MustUseGuiScriptProcessor(
        "Factory", "Organic Texture 2.8"));
      Assert.That(settings.MustUseGuiScriptProcessor(
        "Organic Keys", "Acoustic Mood"));
      Assert.That(!settings.MustUseGuiScriptProcessor(
        "Organic Pads", "Nature"));
    } finally {
      SettingsTestHelper.DeleteAnyData();
    }
  }
}