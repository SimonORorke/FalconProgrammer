using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests;

public static class SettingsTestHelper {
  public const string BatchScriptsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Batch";

  private const string DefaultSettingsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Settings";

  public static string DefaultTemplatePath => Path.Combine(
    TemplateProgramsFolderPath, @"Factory\Keys\DX Mania.uvip");

  public const string OriginalProgramsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Original Programs";

  public const string ProgramsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Programs";

  public const string TemplateProgramsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Template Programs";

  public const string TestApplicationName = "TestFalconProgrammer";

  public static string TestSettingsFolderPath { get; } = Path.Combine(
    TestContext.CurrentContext.TestDirectory, TestApplicationName);

  public static void DeleteAnyData() {
    var settingsFile = Settings.GetSettingsFile(
      TestSettingsFolderPath);
    if (settingsFile.Exists) {
      settingsFile.Delete();
    }
    if (Directory.Exists(TestSettingsFolderPath)) {
      Directory.Delete(TestSettingsFolderPath);
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
    // Restore production settings location.
    var location = SettingsFolderLocation.Read(); // Default application name
    location.Path = DefaultSettingsFolderPath;
    location.Write(); // Default application name
  }

  public static Settings ReadSettings() {
    return Settings.Read(TestSettingsFolderPath, TestApplicationName);
  }

  /// <summary>
  ///   If kept up to date, this should generate the required production settings.
  /// </summary>
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public static void WriteSettings(Settings settings) {
    settings.BatchScriptsFolder = new Settings.Folder {
      Path = BatchScriptsFolderPath
    };
    settings.ProgramsFolder = new Settings.Folder {
      Path = ProgramsFolderPath
    };
    settings.OriginalProgramsFolder = new Settings.Folder {
      Path = OriginalProgramsFolderPath
    };
    settings.TemplateProgramsFolder = new Settings.Folder {
      Path = TemplateProgramsFolderPath
    };
    settings.DefaultTemplate = new Settings.Template {
      Path = DefaultTemplatePath
    };
    settings.MustUseGuiScriptProcessorCategories.Add(
      new Settings.ProgramCategory {
        SoundBank = "Factory",
        Category = "Organic Texture 2.8"
      });
    settings.MustUseGuiScriptProcessorCategories.Add(
      new Settings.ProgramCategory {
        SoundBank = "Organic Keys"
      });
    settings.MustUseGuiScriptProcessorCategories.Add(
      new Settings.ProgramCategory {
        SoundBank = "Pulsar"
      });
    settings.MustUseGuiScriptProcessorCategories.Add(
      new Settings.ProgramCategory {
        SoundBank = "Voklm"
      });
    settings.MidiForMacros.ModWheelReplacementCcNo = 34;
    settings.MidiForMacros.ContinuousCcNoRanges = [
      new Settings.IntegerRange { Start = 31, End = 34 },
      new Settings.IntegerRange { Start = 11, End = 11 },
      new Settings.IntegerRange { Start = 36, End = 37 },
      new Settings.IntegerRange { Start = 28, End = 28 },
      new Settings.IntegerRange { Start = 41, End = 48 },
      new Settings.IntegerRange { Start = 51, End = 58 },
      new Settings.IntegerRange { Start = 61, End = 68 }
    ];
    settings.MidiForMacros.ToggleCcNoRanges = [
      new Settings.IntegerRange { Start = 112, End = 112 }
    ];
    settings.Write();
  }
}