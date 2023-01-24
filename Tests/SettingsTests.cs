using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.Tests;

[TestFixture]
public class SettingsTests {
  [Test]
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public void Test1() {
    const string programsFolderPath =
      @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\Programs";
    const string templatesFolderPath =
      @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\Program Templates";
    DeleteAnyTestData();
    try {
      var settings = ReadTestSettings();
      Assert.That(!File.Exists(settings.SettingsPath));
      Assert.IsEmpty(settings.ProgramCategories);
      settings.ProgramsFolder = new Settings.Folder {
        Path = programsFolderPath
      };
      // Default template
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Factory\Keys\DX Mania.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Factory",
        Category = "Brutal Bass 2.1",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Factory\Brutal Bass 2.1\808 Line.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Factory",
        Category = "Lo-Fi 2.5",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Factory\Lo-Fi 2.5\BAS Gameboy Bass.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Factory",
        Category = "Organic Texture 2.8",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Factory\Organic Texture 2.8\BAS Biggy.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Factory",
        Category = "RetroWave 2.5",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Factory\Lo-Fi 2.5\BAS Gameboy Bass.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Factory",
        Category = "VCF-20 Synths 2.5",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Factory\Lo-Fi 2.5\BAS Gameboy Bass.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Fluidity",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Fluidity\Strings\Guitar Stream.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Hypnotic Drive",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Hypnotic Drive\Leads\Lead - Acid Gravel.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Inner Dimensions",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Inner Dimensions\Key\Melodist.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Organic Keys",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Organic Keys\Acoustic Mood\A Rhapsody.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Pulsar",
        Category = "Bass",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Pulsar\Bass\Warped.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Pulsar",
        Category = "Leads",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Pulsar\Leads\Autumn Rust.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Pulsar",
        Category = "Pads",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Pulsar\Pads\Lore.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Pulsar",
        Category = "Plucks",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Pulsar\Plucks\Resonator.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Savage",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Savage\Leads\Plucker.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Titanium",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Titanium\Keys\Wood Chill.uvip")
      });
      settings.ProgramCategories.Add(new Settings.ProgramCategory {
        SoundBank = "Voklm",
        IsInfoPageLayoutInScript = true,
        TemplatePath = Path.Combine(templatesFolderPath,
          @"Voklm\Synth Choirs\Breath Five.uvip")
      });
      settings.Write();
      settings = ReadTestSettings();
      Assert.That(File.Exists(settings.SettingsPath));
      Assert.That(settings.ProgramsFolder.Path, Is.EqualTo(programsFolderPath));
      Assert.That(settings.ProgramCategories, Has.Count.EqualTo(17));
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