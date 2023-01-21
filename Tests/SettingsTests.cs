namespace FalconProgrammer.Tests;

[TestFixture]
public class SettingsTests {
  [Test]
  public void Test1() {
    const string programsFolderPath =
      @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\Programs";
    DeleteAnyTestData();
    try {
      var settings = ReadTestSettings();
      Assert.That(!File.Exists(settings.SettingsPath));
      Assert.IsEmpty(settings.ProgramTemplates);
      settings.ProgramsFolder = new Settings.Folder {
        Path = programsFolderPath
      };
      settings.ProgramTemplates.Add(new Settings.ProgramTemplate {
        SoundBank = "Factory",
        Category = "RetroWave 2.5",
        // ReSharper disable once StringLiteralTypo
        Path =
          @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\Program Templates\Factory\Lo-Fi 2.5\BAS Gameboy Bass.uvip"
      });
      settings.Write();
      settings = ReadTestSettings();
      Assert.That(File.Exists(settings.SettingsPath));
      Assert.That(settings.ProgramsFolder.Path, Is.EqualTo(programsFolderPath));
      Assert.That(settings.ProgramTemplates, Has.Count.EqualTo(1));
    } finally {
      DeleteAnyTestData();
    }
  }

  private static Settings ReadTestSettings() {
    return Settings.Read(SettingsFolderLocationTests.SettingsFolderPath,
      SettingsFolderLocationTests.TestApplicationName); 
  }

  private static void DeleteAnyTestData() {
    var settingsFile = Settings.GetSettingsFile(
      SettingsFolderLocationTests.SettingsFolderPath);
    if (settingsFile.Exists) {
      settingsFile.Delete();
    }
    SettingsFolderLocationTests.DeleteAnyTestData();
  }
}