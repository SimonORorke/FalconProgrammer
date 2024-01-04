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
      // Debug.WriteLine(
      //   $"TestSettingsFolderPath = '{SettingsTestHelper.TestSettingsFolderPath}'");
      Assert.That(File.Exists(settings.SettingsPath));
      Assert.That(settings.BatchScriptsFolder.Path, Is.EqualTo(
        SettingsTestHelper.BatchScriptsFolderPath));
      Assert.That(settings.ProgramsFolder.Path, Is.EqualTo(
        SettingsTestHelper.ProgramsFolderPath));
      Assert.That(settings.OriginalProgramsFolder.Path, Is.EqualTo(
        SettingsTestHelper.OriginalProgramsFolderPath));
      Assert.That(settings.TemplateProgramsFolder.Path, Is.EqualTo(
        SettingsTestHelper.TemplateProgramsFolderPath));
      Assert.That(settings.DefaultTemplate.Path, Is.EqualTo(
        SettingsTestHelper.DefaultTemplatePath));
      Assert.That(settings.MustUseGuiScriptProcessorCategories, Has.Count.EqualTo(4));
      Assert.That(!settings.MustUseGuiScriptProcessor(
        "Factory", "Bass-Sub"));
      Assert.That(settings.MustUseGuiScriptProcessor(
        "Factory", "Organic Texture 2.8"));
      Assert.That(settings.MustUseGuiScriptProcessor(
        "Organic Keys", "Acoustic Mood"));
      Assert.That(!settings.MustUseGuiScriptProcessor(
        "Organic Pads", "Nature"));
      Assert.That(settings.MidiForMacros.ModWheelReplacementCcNo, Is.EqualTo(34));
      Assert.That(settings.MidiForMacros.ContinuousCcNoRanges, Has.Count.EqualTo(7));
      Assert.That(settings.MidiForMacros.ContinuousCcNoRanges[0].Start, Is.EqualTo(31));
      Assert.That(settings.MidiForMacros.ContinuousCcNoRanges[0].End, Is.EqualTo(34));
      Assert.That(settings.MidiForMacros.ToggleCcNoRanges, Has.Count.EqualTo(1));
      Assert.That(settings.MidiForMacros.ToggleCcNoRanges[0].Start, Is.EqualTo(112));
      Assert.That(settings.MidiForMacros.ToggleCcNoRanges[0].End, Is.EqualTo(112));
    } finally {
      SettingsTestHelper.DeleteAnyData();
    }
  }

  [Test]
  public void Update() {
    SettingsTestHelper.DeleteAnyData();
    try {
      var settings = SettingsTestHelper.ReadSettings();
      Assert.That(!File.Exists(settings.SettingsPath));
      Assert.That(settings.ProgramsFolder.Path, Is.EqualTo(string.Empty));
      settings.ProgramsFolder.Path = SettingsTestHelper.ProgramsFolderPath;
      Assert.That(settings.ProgramsFolder.Path, Is.EqualTo(
        SettingsTestHelper.ProgramsFolderPath));
    } finally {
      SettingsTestHelper.DeleteAnyData();
    }
  }
}