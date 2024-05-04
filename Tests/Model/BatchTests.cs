namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class BatchTests {
  [SetUp]
  public void Setup() {
    Batch = new TestBatch();
  }

  private TestBatch Batch { get; set; } = null!;

  [Test]
  public void CannotReplaceModWheelWithMacroForCategory() {
    const string soundBankName = "Factory";
    const string category = "Organic Texture 2.8";
    Batch.TestSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    Assert.That(Batch.Settings.MidiForMacros.HasModWheelReplacementCcNo);
    Assert.That(Batch.Settings.MustUseGuiScriptProcessor(soundBankName, category));
    Batch.ReplaceModWheelWithMacro(soundBankName, category);
    Assert.That(Batch.MockBatchLog.Lines, Has.Count.EqualTo(1));
    Assert.That(Batch.MockBatchLog.Lines[0], Does.EndWith(
      "because the category's GUI has to be defined in a script."));
  }

  [Test]
  public void CannotReplaceModWheelWithMacroForSoundBank() {
    const string soundBankName = "Organic Keys";
    Batch.TestSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    Assert.That(Batch.Settings.MidiForMacros.HasModWheelReplacementCcNo);
    Assert.That(Batch.Settings.MustUseGuiScriptProcessor(soundBankName));
    Batch.ReplaceModWheelWithMacro(soundBankName);
    Assert.That(Batch.MockBatchLog.Lines, Has.Count.EqualTo(1));
    Assert.That(Batch.MockBatchLog.Lines[0], Does.EndWith(
      "because the sound bank's GUI has to be defined in a script."));
  }

  [Test]
  public void CannotReplaceModWheelWithoutModWheelReplacementCcNo() {
    const string soundBankName = "Organic Keys";
    Batch.TestSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    Batch.Settings.MidiForMacros.ModWheelReplacementCcNo = 0;
    Batch.ReplaceModWheelWithMacro(soundBankName);
    Assert.That(Batch.MockBatchLog.Lines, Has.Count.EqualTo(1));
    Assert.That(Batch.MockBatchLog.Lines[0], Does.EndWith(
      "CC number greater than 1 has not been specified."));
  }

  [Test]
  public void OriginalProgramsFolderFound() {
    Batch.TestSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    Assert.That(Batch.GetOriginalProgramsFolderPath(), Is.EqualTo(
      Batch.Settings.OriginalProgramsFolder.Path));
  }

  [Test]
  public void OriginalProgramsFolderNotFound() {
    Batch.TestSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    Batch.MockFileSystemService.Folder.ExpectedExists = false;
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.GetOriginalProgramsFolderPath());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith(
      "Cannot find original programs folder '"));
  }

  [Test]
  public void OriginalProgramsFolderNotSpecified() {
    Batch.TestSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    Batch.Settings.OriginalProgramsFolder.Path = string.Empty;
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.GetOriginalProgramsFolderPath());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith(
      "The original programs folder is not specified in settings file "));
  }

  [Test]
  public void ProgramsFolderNotFound() {
    Batch.TestSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    Batch.MockFileSystemService.Folder.ExpectedExists = false;
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.QueryCountMacros(null));
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith(
      "Cannot find programs folder '"));
  }

  [Test]
  public void ProgramsFolderNotSpecified() {
    Batch.TestSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    Batch.Settings.ProgramsFolder.Path = string.Empty;
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.QueryCountMacros(null));
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith(
      "The programs folder is not specified in settings file "));
  }

  [Test]
  public void RunScriptForAll() {
    var mockFolderService = Batch.MockFileSystemService.Folder;
    string programsFolderPath = Batch.Settings.ProgramsFolder.Path;
    const string onlySoundBankName = "Fluidity";
    mockFolderService.ExpectedSubfolderNames.Add(
      programsFolderPath, [onlySoundBankName]);
    string onlySoundBankFolderPath = Path.Combine(programsFolderPath, onlySoundBankName);
    const string onlyCategoryName = "Electronic";
    mockFolderService.ExpectedSubfolderNames.Add(
      onlySoundBankFolderPath, [onlyCategoryName]);
    string onlyCategoryFolderPath =
      Path.Combine(onlySoundBankFolderPath, onlyCategoryName);
    mockFolderService.ExpectedFilePaths.Add(
      onlyCategoryFolderPath, ["Cream Synth.uvip", "Fluid Sweeper.uvip"]);
    Batch.TestBatchScriptReaderEmbedded.EmbeddedFileName = "QueriesForAll.xml";
    Batch.RunScript("This will be ignored.xml");
    Assert.That(Batch.MockBatchLog.Lines, Has.Count.EqualTo(2));
    Assert.That(Batch.MockBatchLog.Lines[0], Is.EqualTo(
      @"QueryReverbTypes: 'Fluidity\Electronic\Cream Synth'"));
    Assert.That(Batch.MockBatchLog.Lines[1], Is.EqualTo(
      @"QueryReverbTypes: 'Fluidity\Electronic\Fluid Sweeper'"));
  }

  [Test]
  public void RunScriptForProgram() {
    Batch.TestBatchScriptReaderEmbedded.EmbeddedFileName = "QueriesForProgram.xml";
    Batch.RunScript("This will be ignored.xml");
    Assert.That(Batch.MockBatchLog.Lines, Has.Count.EqualTo(2));
    Assert.That(Batch.MockBatchLog.Lines[0], Is.EqualTo(@"QueryAdsrMacros: 'SB\Cat\P1'"));
    Assert.That(Batch.MockBatchLog.Lines[1], Is.EqualTo(@"QueryDelayTypes: 'SB\Cat\P1'"));
  }

  [Test]
  public void SoundBankFolderNotFound() {
    Batch.TestSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    Batch.MockFileSystemService.Folder.ExistingPaths.Add(
      Batch.Settings.ProgramsFolder.Path);
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.QueryCountMacros("Factory"));
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith("Cannot find sound bank folder '"));
  }
}