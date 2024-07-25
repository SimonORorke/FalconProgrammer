using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.Tests.Model;

public static class SettingsTestHelper {
  public const string DefaultSettingsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Settings";

  public const string OriginalProgramsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Original Programs";

  public const string ProgramsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Programs";

  public const string TemplateProgramsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Template Programs";

  public const string TestAppDataFolderName = "TestFalconProgrammer";

  public static string TestSettingsFolderPath { get; } = Path.Combine(
    TestContext.CurrentContext.TestDirectory, TestAppDataFolderName);

  public static void DeleteAnyData() {
    var settingsFile = new FileInfo(
      Settings.GetSettingsPath(TestSettingsFolderPath));
    if (settingsFile.Exists) {
      settingsFile.Delete();
    }
    if (Directory.Exists(TestSettingsFolderPath)) {
      Directory.Delete(TestSettingsFolderPath);
    }
    var locationFile = new FileInfo(
      SettingsFolderLocation.GetSettingsFolderLocationPath(TestAppDataFolderName));
    if (locationFile.Exists) {
      locationFile.Delete();
    }
    string appDataFolderPath = SettingsFolderLocation.GetAppDataFolderPath(
      TestAppDataFolderName);
    if (Directory.Exists(appDataFolderPath)) {
      Directory.Delete(appDataFolderPath);
    }
    // Restore production settings location. Default application name.
    var settingsFolderLocationReader = new SettingsFolderLocationReader();
    var location = settingsFolderLocationReader.Read();
    location.Path = DefaultSettingsFolderPath;
    location.Write();
  }

  public static Settings ReadSettings() {
    var settingsReader = new TestSettingsReaderReal {
      AppDataFolderName = TestAppDataFolderName,
      DefaultSettingsFolderPath = TestSettingsFolderPath
    };
    return settingsReader.Read();
  }

  /// <summary>
  ///   If kept up to date, this should generate the required production settings.
  /// </summary>
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public static void WriteSettings(Settings settings) {
    settings.ProgramsFolder = new Folder {
      Path = ProgramsFolderPath
    };
    settings.OriginalProgramsFolder = new Folder {
      Path = OriginalProgramsFolderPath
    };
    settings.TemplateProgramsFolder = new Folder {
      Path = TemplateProgramsFolderPath
    };
    settings.MustUseGuiScriptProcessorCategories.Add(
      new SoundBankCategorySetting {
        SoundBank = "Falcon Factory",
        Category = "Organic Texture 2.8"
      });
    settings.MustUseGuiScriptProcessorCategories.Add(
      new SoundBankCategorySetting {
        SoundBank = "Organic Keys"
      });
    settings.MustUseGuiScriptProcessorCategories.Add(
      new SoundBankCategorySetting {
        SoundBank = "Pulsar"
      });
    settings.MustUseGuiScriptProcessorCategories.Add(
      new SoundBankCategorySetting {
        SoundBank = "Voklm"
      });
    settings.MidiForMacros.ModWheelReplacementCcNo = 34;
    settings.MidiForMacros.ContinuousCcNoRanges = [
      new IntegerRange { Start = 31, End = 34 },
      new IntegerRange { Start = 11, End = 11 },
      new IntegerRange { Start = 36, End = 37 },
      new IntegerRange { Start = 28, End = 28 },
      new IntegerRange { Start = 41, End = 48 },
      new IntegerRange { Start = 51, End = 58 },
      new IntegerRange { Start = 61, End = 68 }
    ];
    settings.MidiForMacros.ToggleCcNoRanges = [
      new IntegerRange { Start = 112, End = 112 }
    ];
    settings.Write();
  }
}