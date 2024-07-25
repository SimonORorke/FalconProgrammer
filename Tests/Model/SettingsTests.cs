using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class SettingsTests {
  [Test]
  public void AppendCcNoToMacroDisplayNames() {
    var settingsReader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "LocationsSettings.xml"
    };
    var settings = settingsReader.Read();
    Assert.That(settings.MidiForMacros.AppendCcNoToMacroDisplayNames, Is.True);
    settings.MidiForMacros.AppendCcNoToMacroDisplayNames = false;
    settings.Write();
    var writtenSettings =
      (Settings)settingsReader.MockSerialiserForSettings.LastObjectSerialised;
    Assert.That(writtenSettings.MidiForMacros.AppendCcNoToMacroDisplayNames, Is.False);
  }
  
  [Test]
  public void Background() {
    var settingsReader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "DefaultSettingsWithMidi.xml"
    };
    var settings = settingsReader.Read();
    const string soundBank = "Eternal Funk";
    Assert.That(settings.TryGetSoundBankBackgroundImagePath(
      soundBank, out _), Is.False);
    settingsReader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "BatchSettings.xml"
    };
    settings = settingsReader.Read();
    Assert.That(settings.TryGetSoundBankBackgroundImagePath(
      soundBank, out string path), Is.True);
    Assert.That(path, Does.EndWith("Yellowish Mid-Green.png"));
  }

  [Test]
  public void Batch() {
    var settingsReader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "LocationsSettings.xml"
    };
    var settings = settingsReader.Read();
    Assert.That(settings.Batch.Scope.SoundBank, Is.Empty);
    Assert.That(settings.Batch.Scope.Category, Is.Empty);
    Assert.That(settings.Batch.Scope.Program, Is.Empty);
    Assert.That(settings.Batch.Tasks, Is.Empty);
    const string soundBank = "Pulsar";
    const string category = "Plucks";
    const string program = "Music Box";
    const string task = nameof(ConfigTask.AssignMacroCcs);
    settings.Batch.Scope.SoundBank = soundBank;
    settings.Batch.Scope.Category = category;
    settings.Batch.Scope.Program = program;
    settings.Batch.Tasks.Add(task);
    settings.Write();
    var writtenSettings =
      (Settings)settingsReader.MockSerialiserForSettings.LastObjectSerialised;
    Assert.That(writtenSettings.Batch.Scope.SoundBank, Is.EqualTo(soundBank));
    Assert.That(writtenSettings.Batch.Scope.Category, Is.EqualTo(category));
    Assert.That(writtenSettings.Batch.Scope.Program, Is.EqualTo(program));
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
    Assert.That(settings.DoNotZeroReverb, Is.Empty);
    const string soundBank = "Pulsar";
    const string category = "Plucks";
    const string program = "Music Box";
    Assert.That(settings.CanChangeReverbToZero(soundBank, category, program), Is.True);
    var programPath = new ProgramPath {
      SoundBank = soundBank,
      Category = category,
      Program = program
    };
    settings.DoNotZeroReverb.Add(programPath);
    settings.Write();
    var mockSerialiser = settingsReader.MockSerialiserForSettings;
    var writtenSettings = (Settings)mockSerialiser.LastObjectSerialised;
    Assert.That(writtenSettings.DoNotZeroReverb, Has.Count.EqualTo(1));
    Assert.That(
      writtenSettings.DoNotZeroReverb[0].SoundBank, Is.EqualTo(soundBank));
    Assert.That(
      writtenSettings.DoNotZeroReverb[0].Category, Is.EqualTo(category));
    Assert.That(
      writtenSettings.DoNotZeroReverb[0].Program, Is.EqualTo(program));
    Assert.That(mockSerialiser.LastOutputText, Does.Contain("<DoNotZeroReverb>"));
    Assert.That(settings.CanChangeReverbToZero(soundBank, category, program), Is.False);
  }

  [Test]
  public void GetNextGetNextContinuousCcNo() {
    var settingsReader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "DefaultSettingsWithMidi.xml"
    };
    var settings = settingsReader.Read();
    settings.MidiForMacros.CurrentContinuousCcNo = 0;
    int nextCcNo = settings.MidiForMacros.GetNextContinuousCcNo(false);
    Assert.That(nextCcNo, Is.EqualTo(31));
    nextCcNo = settings.MidiForMacros.GetNextContinuousCcNo(false);
    Assert.That(nextCcNo, Is.EqualTo(32));
    settings.MidiForMacros.CurrentContinuousCcNo = 1;
    nextCcNo = settings.MidiForMacros.GetNextContinuousCcNo(false);
    Assert.That(nextCcNo, Is.EqualTo(11));
    settings.MidiForMacros.CurrentContinuousCcNo = 34;
    nextCcNo = settings.MidiForMacros.GetNextContinuousCcNo(true);
    Assert.That(nextCcNo, Is.EqualTo(1));
  }

  [Test]
  public void GetNextToggleCcNo() {
    var settingsReader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "DefaultSettingsWithMidi.xml"
    };
    var settings = settingsReader.Read();
    settings.MidiForMacros.CurrentToggleCcNo = 0;
    int nextCcNo = settings.MidiForMacros.GetNextToggleCcNo();
    Assert.That(nextCcNo, Is.EqualTo(112));
    nextCcNo = settings.MidiForMacros.GetNextToggleCcNo();
    Assert.That(nextCcNo, Is.EqualTo(113));
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
      Assert.That(settings.MustUseGuiScriptProcessorCategories, Has.Count.EqualTo(4));
      Assert.That(!settings.MustUseGuiScriptProcessor(
        "Falcon Factory", "Bass-Sub"));
      Assert.That(settings.MustUseGuiScriptProcessor(
        "Falcon Factory", "Organic Texture 2.8"));
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
        SimulatedExists = false
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
    Assert.That(settings.MustUseGuiScriptProcessorCategories, Has.Count.EqualTo(3));
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