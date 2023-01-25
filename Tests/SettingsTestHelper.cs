using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests;

public static class SettingsTestHelper {
  public const string ProgramsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\Programs";

  [PublicAPI] public const string TemplatesFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\Program Templates";

  public const string TestApplicationName = "TestFalconProgrammer";

  public static string SettingsFolderPath { get; } = Path.Combine(
    TestContext.CurrentContext.TestDirectory, TestApplicationName);

  public static void DeleteAnyData() {
    var settingsFile = Settings.GetSettingsFile(
      SettingsFolderPath);
    if (settingsFile.Exists) {
      settingsFile.Delete();
    }
    if (Directory.Exists(SettingsFolderPath)) {
      Directory.Delete(SettingsFolderPath);
    }
    var locationFile =
      SettingsFolderLocation.GetSettingsFolderLocationFile(TestApplicationName);
    if (locationFile.Exists) {
      locationFile.Delete();
    }
    var appDataFolder = SettingsFolderLocation.GetAppDataFolder(
      TestApplicationName);
    if (appDataFolder.Exists) {
      appDataFolder.Delete();
    }
  }

  public static Settings ReadSettings() {
    return Settings.Read(SettingsFolderPath, TestApplicationName);
  }

  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public static void WriteSettings(Settings settings) {
    settings.ProgramsFolder = new Settings.Folder {
      Path = ProgramsFolderPath
    };
    // Default template
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Factory\Keys\DX Mania.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Factory",
      Category = "Brutal Bass 2.1",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Factory\Brutal Bass 2.1\808 Line.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Factory",
      Category = "Lo-Fi 2.5",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Factory\Lo-Fi 2.5\BAS Gameboy Bass.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Factory",
      Category = "Organic Texture 2.8",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Factory\Organic Texture 2.8\BAS Biggy.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Factory",
      Category = "RetroWave 2.5",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Factory\Lo-Fi 2.5\BAS Gameboy Bass.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Factory",
      Category = "VCF-20 Synths 2.5",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Factory\Lo-Fi 2.5\BAS Gameboy Bass.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Fluidity",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Fluidity\Strings\Guitar Stream.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Hypnotic Drive",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Hypnotic Drive\Leads\Lead - Acid Gravel.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Inner Dimensions",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Inner Dimensions\Key\Melodist.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Organic Keys",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Organic Keys\Acoustic Mood\A Rhapsody.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Pulsar",
      Category = "Bass",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Pulsar\Bass\Warped.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Pulsar",
      Category = "Leads",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Pulsar\Leads\Autumn Rust.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Pulsar",
      Category = "Pads",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Pulsar\Pads\Lore.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Pulsar",
      Category = "Plucks",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Pulsar\Plucks\Resonator.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Savage",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Savage\Leads\Plucker.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Titanium",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Titanium\Keys\Wood Chill.uvip")
    });
    settings.ProgramCategories.Add(new Settings.ProgramCategory {
      SoundBank = "Voklm",
      IsInfoPageLayoutInScript = true,
      TemplatePath = Path.Combine(TemplatesFolderPath,
        @"Voklm\Synth Choirs\Breath Five.uvip")
    });
    settings.Write();
  }
}