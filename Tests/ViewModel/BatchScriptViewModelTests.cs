using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class BatchScriptViewModelTests : ViewModelTestsBase {
  private const string BatchScriptPath = "This path will be ignored.xml";
  private Settings Settings { get; set; } = null!;
  private TestBatchScriptViewModel ViewModel { get; set; } = null!;

  [SetUp]
  public override void Setup() {
    base.Setup();
    MockDialogService.SimulatedPath = BatchScriptPath;
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel = new TestBatchScriptViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  [Test]
  public async Task LoadScript() {
    await ConfigureScript();
    await ViewModel.LoadScriptCommand.ExecuteAsync(null);
    Assert.That(ViewModel.Scope.SoundBank, Is.EqualTo("Spectre"));
    Assert.That(ViewModel.Scope.Category, Is.EqualTo("Bells"));
    Assert.That(ViewModel.Scope.Program, Is.EqualTo("BL Xylophone"));
    Assert.That(ViewModel.Tasks, Has.Count.EqualTo(2));
    Assert.That(ViewModel.Tasks[0].Name, Is.EqualTo("QueryAdsrMacros"));
    await ViewModel.QueryClose();
    Assert.That(ViewModel.Settings.Batch.Scope.SoundBank, Is.EqualTo("Spectre"));
    Assert.That(ViewModel.Settings.Batch.Scope.Category, Is.EqualTo("Bells"));
    Assert.That(ViewModel.Settings.Batch.Scope.Program, Is.EqualTo("BL Xylophone"));
    Assert.That(ViewModel.Settings.Batch.Tasks, Has.Count.EqualTo(1));
    Assert.That(ViewModel.Settings.Batch.Tasks[0], Is.EqualTo("QueryAdsrMacros"));
    Assert.That(ViewModel.Status, Does.StartWith("Loaded script"));
  }

  [Test]
  public async Task LoadScriptXmlError() {
    await ConfigureScript();
    ViewModel.TestBatch.EmbeddedScriptFileName = "XmlErrorScript.xml";
    await ViewModel.LoadScriptCommand.ExecuteAsync(null);
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Invalid XML was found in embedded file"));
  }

  [Test]
  public async Task OriginalProgramsFolderNotFound() {
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.ProgramsFolder.Path);
    ViewModel.AddSoundBankSubfolders(Settings.ProgramsFolder.Path);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.Contain(
      "Batch scripts cannot be run: cannot find original programs folder '"));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task NoProgramsFolder() {
    Settings = ReadMockSettings("DefaultSettingsWithMidi.xml");
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.Contain(
      "Batch scripts cannot be run: the programs folder has not been specified."));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task RunScript() {
    await ConfigureScript();
    ViewModel.RunScriptCommand.Execute(null);
    Assert.That(ViewModel.Log[0], Is.EqualTo(
      @"InitialiseLayout - 'Falcon Factory\Keys\Morning Keys'"));
    Assert.That(ViewModel.Status, Does.StartWith("Run ended"));
    ViewModel.CopyLogCommand.Execute(null);
    Assert.That(ViewModel.SavedLog, Does.Contain(@"Falcon Factory\Keys\Morning Keys"));
    Assert.That(ViewModel.Status, Is.EqualTo("Copied log to clipboard."));
  }

  [Test]
  public async Task RunScriptCancelled() {
    await ConfigureScript();
    // This should cancel the batch immediately, before any Falcon programs are updated.
    ViewModel.TestBatch.UpdatePrograms = true;
    ViewModel.CancelRunCommand.Execute(null);
    ViewModel.RunScriptCommand.Execute(null);
    Assert.That(ViewModel.Log, Does.Contain("The batch run has been cancelled."));
  }

  [Test]
  public async Task SaveScript() {
    await ConfigureScript();
    await ViewModel.SaveScriptCommand.ExecuteAsync(null);
    Assert.That(ViewModel.TestBatchScript, Is.Not.Null);
    Assert.That(ViewModel.TestBatchScript.MockSerialiser.LastOutputPath,
      Is.EqualTo(BatchScriptPath));
    Assert.That(ViewModel.Status, Does.StartWith("Saved script to "));
  }

  [Test]
  public async Task SaveScriptCancelled() {
    await ConfigureScript();
    MockDialogService.Cancel = true;
    await ViewModel.SaveScriptCommand.ExecuteAsync(null);
    Assert.That(ViewModel.TestBatchScript, Is.Null);
  }

  [Test]
  public async Task TemplateProgramsFolderNotFound() {
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.ProgramsFolder.Path);
    ViewModel.AddSoundBankSubfolders(Settings.ProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.OriginalProgramsFolder.Path);
    ViewModel.AddSoundBankSubfolders(Settings.OriginalProgramsFolder.Path);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.Contain(
      "Batch scripts cannot be run: cannot find template programs folder '"));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task UpdateScope() {
    ViewModel.ConfigureValidMockFileSystemService(Settings);
    await ViewModel.Open();
    const string soundBank = "Pulsar";
    const string category = "Plucks";
    const string program = "Music Box";
    Assert.That(ViewModel.Scopes, Has.Count.EqualTo(1));
    Assert.That(ViewModel.Scope.SoundBank, Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(ViewModel.Scope.SoundBanks[0], Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(ViewModel.Scope.Category, Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(ViewModel.Scope.Categories[0], Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(ViewModel.Scope.Program, Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(ViewModel.Scope.Programs[0], Is.EqualTo(SoundBankItem.AllCaption));
    ViewModel.Scope.SoundBank = soundBank;
    ViewModel.Scope.Category = category;
    ViewModel.Scope.Program = program;
    await ViewModel.QueryClose();
    Assert.That(ViewModel.Settings.Batch.Scope.SoundBank, Is.EqualTo(soundBank));
    Assert.That(ViewModel.Settings.Batch.Scope.Category, Is.EqualTo(category));
    Assert.That(ViewModel.Settings.Batch.Scope.Program, Is.EqualTo(program));
  }

  [Test]
  public async Task UpdateTasks() {
    ViewModel.ConfigureValidMockFileSystemService(Settings);
    await ViewModel.Open();
    Assert.That(ViewModel.Settings.Batch.Tasks, Has.Count.EqualTo(2));
    var taskItem = ViewModel.Tasks[0];
    Assert.That(taskItem.Name, Is.EqualTo("InitialiseLayout"));
    const string task = "ReuseCc1";
    taskItem.Name = task;
    await ViewModel.QueryClose();
    Assert.That(ViewModel.Settings.Batch.Tasks[0], Is.EqualTo(task));
  }

  [Test]
  public async Task ValidSettingsOnOpen() {
    ViewModel.ConfigureValidMockFileSystemService(Settings);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(0));
  }

  private async Task ConfigureScript() {
    ViewModel.ConfigureValidMockFileSystemService(Settings);
    await ViewModel.Open();
    const string soundBank = "Falcon Factory";
    const string category = "Keys";
    const string program = "Morning Keys";
    ViewModel.Scope.SoundBank = soundBank;
    ViewModel.Scope.Category = category;
    ViewModel.Scope.Program = program;
  }
}