using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class SettingsTests {
  [Test]
  public void Batch() {
    var settingsReader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "LocationsSettings.xml"
    };
    var settings = settingsReader.Read();
    Assert.That(settings.Batch.SoundBank, Is.Empty);
    Assert.That(settings.Batch.Category, Is.Empty);
    Assert.That(settings.Batch.Program, Is.Empty);
    Assert.That(settings.Batch.Tasks, Is.Empty);
    const string soundBank = "Pulsar";
    const string category = "Plucks";
    const string program = "Music Box";
    const string task = nameof(ConfigTask.UpdateMacroCcs);
    settings.Batch.SoundBank = soundBank;
    settings.Batch.Category = category;
    settings.Batch.Program = program;
    settings.Batch.Tasks.Add(task);
    settings.Write();
    var writtenSettings =
      (Settings)settingsReader.MockSerialiserForSettings.LastObjectSerialised;
    Assert.That(writtenSettings.Batch.SoundBank, Is.EqualTo(soundBank));
    Assert.That(writtenSettings.Batch.Category, Is.EqualTo(category));
    Assert.That(writtenSettings.Batch.Program, Is.EqualTo(program));
    Assert.That(writtenSettings.Batch.Tasks, Has.Count.EqualTo(1));
    Assert.That(writtenSettings.Batch.Tasks[0], Is.EqualTo(task));
  }

  [Test]
  public void ColourScheme() {
    var settingsReader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "LocationsSettings.xml"
    };
    var settings = settingsReader.Read();
    Assert.That(settings.ColourScheme, Is.Empty);
    const string colourScheme = nameof(ColourSchemeId.Lavender);
    settings.ColourScheme = colourScheme;
    settings.Write();
    var writtenSettings =
      (Settings)settingsReader.MockSerialiserForSettings.LastObjectSerialised;
    Assert.That(writtenSettings.ColourScheme, Is.EqualTo(colourScheme));
  }

  [Test]
  public void DoNotZeroReverbMacros() {
    var settingsReader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "LocationsSettings.xml"
    };
    var settings = settingsReader.Read();
    Assert.That(settings.DoNotZeroReverbMacros, Is.Empty);
    const string soundBank = "Pulsar";
    const string category = "Plucks";
    const string program = "Music Box";
    var programPath = new Settings.ProgramPath {
      SoundBank = soundBank,
      Category = category,
      Program = program
    };
    settings.DoNotZeroReverbMacros.Add(programPath);
    settings.Write();
    var mockSerialiser = settingsReader.MockSerialiserForSettings;
    var writtenSettings = (Settings)mockSerialiser.LastObjectSerialised;
    Assert.That(writtenSettings.DoNotZeroReverbMacros, Has.Count.EqualTo(1));
    Assert.That(
      writtenSettings.DoNotZeroReverbMacros[0].SoundBank, Is.EqualTo(soundBank));
    Assert.That(
      writtenSettings.DoNotZeroReverbMacros[0].Category, Is.EqualTo(category));
    Assert.That(
      writtenSettings.DoNotZeroReverbMacros[0].Program, Is.EqualTo(program));
    Assert.That(mockSerialiser.LastOutputText, Does.Contain("<DoNotZeroReverbMacros>"));
  }

  [Test]
  public void RealTestSettingsFile() {
    SettingsTestHelper.DeleteAnyData();
    Settings? settings = null;
    try {
      settings = SettingsTestHelper.ReadSettings();
      Assert.That(!File.Exists(settings.SettingsPath));
      SettingsTestHelper.WriteSettings(settings);
      settings = SettingsTestHelper.ReadSettings();
      // Debug.WriteLine(
      //   $"TestSettingsFolderPath = '{SettingsTestHelper.TestSettingsFolderPath}'");
      Assert.That(File.Exists(settings.SettingsPath));
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
      // Test FileSystemService.Folder.GetSubfolderNames
      const string subfolderName = "Test";
      var subfolder =
        Directory.CreateDirectory(
          Path.Combine(SettingsTestHelper.TestSettingsFolderPath, subfolderName));
      Assert.That(
        settings.FileSystemService.Folder.GetSubfolderNames(
          SettingsTestHelper.TestSettingsFolderPath), Has.Count.EqualTo(1));
      Assert.That(
        settings.FileSystemService.Folder.GetSubfolderNames(
          SettingsTestHelper.TestSettingsFolderPath)[0], Is.EqualTo(subfolderName));
      subfolder.Delete();
    } finally {
      SettingsTestHelper.DeleteAnyData();
      if (settings != null) {
        Assert.Throws<DirectoryNotFoundException>(
          () => settings.FileSystemService.Folder.GetSubfolderNames(
            SettingsTestHelper.TestSettingsFolderPath));
      }
    }
  }

  [Test]
  public void UseDefault() {
    var mockFileSystemService = new MockFileSystemService {
      File = {
        ExpectedExists = false
      }
    };
    var settingsReader = new SettingsReader {
      AppDataFolderName = SettingsTestHelper.TestAppDataFolderName,
      FileSystemService = mockFileSystemService,
      Serialiser = new MockSerialiser()
    };
    var settings = settingsReader.Read();
    Assert.That(settings.ProgramsFolder.Path, Is.Empty);
    Assert.That(settings.MustUseGuiScriptProcessorCategories, Is.Empty);
    settings = settingsReader.Read(true);
    Assert.That(settings.ProgramsFolder.Path, Is.Empty);
    Assert.That(settings.MustUseGuiScriptProcessorCategories, Has.Count.EqualTo(5));
  }

  [Test]
  public void WriteToNewSettingsFolder() {
    var mockSerializer = new MockSerialiser();
    var settings = new Settings {
      Serialiser = mockSerializer,
      SettingsPath = @"K:\Libraries\Settings.xml"
    };
    const string newSettingsFolderPath = @"K:\Markup";
    const string newSettingsPath = @"K:\Markup\Settings.xml";
    settings.Write(newSettingsFolderPath);
    Assert.That(settings.SettingsPath, Is.EqualTo(newSettingsPath));
    Assert.That(mockSerializer.SerializeCount, Is.EqualTo(1));
    Assert.That(mockSerializer.LastType, Is.EqualTo(typeof(Settings)));
    Assert.That(mockSerializer.LastObjectSerialised, Is.SameAs(settings));
    Assert.That(mockSerializer.LastOutputPath, Is.EqualTo(newSettingsPath));
  }
}