namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class BatchTests {
  [SetUp]
  public void Setup() {
    Batch = new TestBatch();
  }

  private TestBatch Batch { get; set; } = null!;

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
}