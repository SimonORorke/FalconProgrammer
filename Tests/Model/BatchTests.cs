using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   For each Test here, modifications of Falcon programs are mocked out by setting
///   <see cref="TestBatch.UpdatePrograms" /> to false. Tests where
///   <see cref="TestBatch" /> is used to facilitate tests of
///   <see cref="FalconProgram "/> should go in <see cref="FalconProgramTests" />.
/// </summary>
[TestFixture]
public class BatchTests {
  [SetUp]
  public void Setup() {
    Batch = new TestBatch {
      // We don't want to test modifications of Falcon programs here.
      // Those tests should go in FalconProgramTests.
      UpdatePrograms = false,
      TestSettingsReaderEmbedded = {
        EmbeddedFileName = "BatchSettings.xml"
      }
    };
  }

  private TestBatch Batch { get; set; } = null!;
  private const string BatchScriptPath = "This path X will be ignored.xml";
  
  private CancellationTokenSource RunCancellationTokenSource { get; } =
    new CancellationTokenSource();

  [Test]
  public void CannotReplaceModWheelWithMacroForCategory() {
    const string soundBankName = "Factory";
    const string category = "Organic Texture 2.8";
    Assert.That(Batch.Settings.MidiForMacros.HasModWheelReplacementCcNo);
    Assert.That(Batch.Settings.MustUseGuiScriptProcessor(soundBankName, category));
    Batch.EmbeddedProgramFileName = "GuiScriptProcessor.uvip";
    Batch.EmbeddedTemplateFileName = "GuiScriptProcessor.uvip";
    Batch.ReplaceModWheelWithMacro(soundBankName, category);
    Assert.That(Batch.MockBatchLog.Lines, Has.Count.EqualTo(1));
    Assert.That(Batch.MockBatchLog.Lines[0], Does.EndWith(
      "because the category's GUI has to be defined in a script."));
  }

  [Test]
  public void CannotReplaceModWheelWithMacroForSoundBank() {
    const string soundBankName = "Organic Keys";
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
    Batch.Settings.MidiForMacros.ModWheelReplacementCcNo = 0;
    Batch.ReplaceModWheelWithMacro(soundBankName);
    Assert.That(Batch.MockBatchLog.Lines, Has.Count.EqualTo(1));
    Assert.That(Batch.MockBatchLog.Lines[0], Does.EndWith(
      "CC number greater than 1 has not been specified."));
  }

  [Test]
  public void OriginalProgramsFolderFound() {
    Assert.That(Batch.GetOriginalProgramsFolderPath(), Is.EqualTo(
      Batch.Settings.OriginalProgramsFolder.Path));
  }

  [Test]
  public void OriginalProgramsFolderNotFound() {
    Batch.MockFileSystemService.Folder.ExpectedExists = false;
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.GetOriginalProgramsFolderPath());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith(
      "Cannot find original programs folder '"));
  }

  [Test]
  public void OriginalProgramsFolderNotSpecified() {
    Batch.Settings.OriginalProgramsFolder.Path = string.Empty;
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.GetOriginalProgramsFolderPath());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith(
      "The original programs folder is not specified in settings file "));
  }

  [Test]
  public void ProgramsFolderNotFound() {
    Batch.MockFileSystemService.Folder.ExpectedExists = false;
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.QueryCountMacros(null));
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith(
      "Cannot find programs folder '"));
  }

  [Test]
  public void ProgramsFolderNotSpecified() {
    Batch.Settings.ProgramsFolder.Path = string.Empty;
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.UpdateMacroCcs(null));
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith(
      "The programs folder is not specified in settings file "));
  }

  [Test]
  public void RunScriptCancelled() {
    RunCancellationTokenSource.CancelAsync();
    Batch.UpdatePrograms = true;
    Batch.RunScript(BatchScriptPath, RunCancellationTokenSource.Token);
    Assert.That(Batch.HasScriptRunEnded);
    Assert.That(Batch.MockBatchLog.Text, Does.Contain(
      "The batch run has been cancelled."));
  }

  [Test]
  public void RunScriptException() {
    const string errorMessage = "This is a test error message.";
    // Simulate FalconProgram throwing an ApplicationException when a task is run.
    Batch.ExceptionWhenConfiguringProgram = new ApplicationException(errorMessage);
    Batch.RunScript(BatchScriptPath, RunCancellationTokenSource.Token);
    Assert.That(Batch.HasScriptRunEnded);
    Assert.That(Batch.MockBatchLog.Text, Does.Contain(errorMessage));
    // The log should contain the error message but not a stack trace.
    Assert.That(Batch.MockBatchLog.Text, Does.Not.Contain(nameof(ApplicationException)));
    // Simulate FalconProgram throwing an Exception other than an ApplicationException
    // when a task is run.
    Batch.MockBatchLog.Lines.Clear();
    Batch.ExceptionWhenConfiguringProgram = new InvalidOperationException(errorMessage);
    Batch.RunScript(BatchScriptPath, RunCancellationTokenSource.Token);
    Assert.That(Batch.HasScriptRunEnded);
    Assert.That(Batch.MockBatchLog.Text, Does.Contain(errorMessage));
    // The log should contain a stack trace.
    Assert.That(Batch.MockBatchLog.Text, Does.Contain(nameof(InvalidOperationException)));
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
    Batch.EmbeddedScriptFileName = "QueriesForAll.xml";
    Batch.RunScript(BatchScriptPath, RunCancellationTokenSource.Token);
    Assert.That(Batch.MockBatchLog.Lines, Has.Count.EqualTo(4));
    Assert.That(Batch.MockBatchLog.Lines[0], Is.EqualTo("Reverb Types:"));
    Assert.That(Batch.MockBatchLog.Lines[1], Is.EqualTo(
      @"QueryReverbTypes: 'Fluidity\Electronic\Cream Synth'"));
    Assert.That(Batch.MockBatchLog.Lines[2], Is.EqualTo(
      @"QueryReverbTypes: 'Fluidity\Electronic\Fluid Sweeper'"));
    Assert.That(Batch.MockBatchLog.Lines[3], Is.EqualTo("The batch run has finished."));
  }

  [Test]
  public void RunScriptForProgram() {
    Batch.RunScript(BatchScriptPath, RunCancellationTokenSource.Token);
    Assert.That(Batch.HasScriptRunEnded);
    Assert.That(Batch.MockBatchLog.Lines, Has.Count.EqualTo(4));
    Assert.That(Batch.MockBatchLog.Lines[0], Is.EqualTo(@"QueryAdsrMacros: 'SB\Cat\P1'"));
    Assert.That(Batch.MockBatchLog.Lines[1], Is.EqualTo("Delay Types:"));
    Assert.That(Batch.MockBatchLog.Lines[2], Is.EqualTo(@"QueryDelayTypes: 'SB\Cat\P1'"));
    Assert.That(Batch.MockBatchLog.Lines[3], Is.EqualTo("The batch run has finished."));
  }

  [Test]
  public void SoundBankFolderNotFound() {
    Batch.MockFileSystemService.Folder.ExistingPaths.Add(
      Batch.Settings.ProgramsFolder.Path);
    var exception = Assert.Throws<ApplicationException>(
      () => Batch.PrependPathLineToDescription("Factory"));
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith("Cannot find sound bank folder '"));
  }
}