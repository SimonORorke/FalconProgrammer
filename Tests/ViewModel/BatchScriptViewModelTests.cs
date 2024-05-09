﻿using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class BatchScriptViewModelTests : ViewModelTestsBase {
  private const string BatchScriptPath = "This path will be ignored.xml";
  private Settings Settings { get; set; } = null!;
  private TestBatchScriptViewModel ViewModel { get; set; } = null!;

  [SetUp]
  public override void Setup() {
    base.Setup();
    MockDialogService.ExpectedPath = BatchScriptPath;
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel = new TestBatchScriptViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  [Test]
  public async Task DefaultTemplateFileNotFound() {
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.ProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.OriginalProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.OriginalProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.TemplateProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.TemplateProgramsFolder.Path);
    MockFileSystemService.File.ExistingPaths.Add(
      @"J:\FalconProgrammer\Settings\Settings.xml");
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.Contain(
      "Batch scripts cannot be run: cannot find default template file '"));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task OriginalProgramsFolderNotFound() {
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.ProgramsFolder.Path);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.Contain(
      "Batch scripts cannot be run: cannot find original programs folder '"));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task NoDefaultTemplateFile() {
    Settings = ReadMockSettings("NoDefaultTemplate.xml");
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.ProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.OriginalProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.OriginalProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.TemplateProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.TemplateProgramsFolder.Path);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.Contain(
      "Batch scripts cannot be run: the default template file has not been specified."));
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
  public async Task RunSavedScript() {
    Assert.That(ViewModel.CanRunSavedScript);
    await ViewModel.RunSavedScriptCommand.ExecuteAsync(null);
    // Commands are disabled while a script is running. That would be fiddly to test.
    Assert.That(ViewModel.CanRunSavedScript);
    Assert.That(ViewModel.Log[0], Is.EqualTo(@"QueryAdsrMacros: 'SB\Cat\P1'"));
  }

  [Test]
  public async Task RunThisScript() {
    await ConfigureThisScript();
    Assert.That(ViewModel.CanRunThisScript);
    Assert.That(ViewModel.CanSaveLog);
    await ViewModel.RunThisScriptCommand.ExecuteAsync(null);
    // Commands are disabled while a script is running. That would be fiddly to test.
    Assert.That(ViewModel.CanRunThisScript);
    Assert.That(ViewModel.CanSaveLog);
    Assert.That(ViewModel.Log[0], Is.EqualTo(
      @"InitialiseLayout: 'Factory\Keys\Morning Keys'"));
    await ViewModel.SaveLogCommand.ExecuteAsync(null);
    Assert.That(ViewModel.SavedLog, Is.EqualTo(ViewModel.BatchLog.ToString()));
  }

  [Test]
  public async Task RunThisScriptCancelled() {
    await ConfigureThisScript();
    Assert.That(!ViewModel.CanCancelBatchRun);
    // The Cancel command will be enabled while the batch is running.
    // However, that's tricky to test. Fortunately, we can ignore the fact that the
    // command is disabled at this stage, which will only prevent it from being executed
    // in the GUI. So we will cancel before running, which should be impossible in the
    // GUI. This should cancel the batch immediately, before any Falcon programs are
    // updated.
    ViewModel.TestBatch.UpdatePrograms = true;
    ViewModel.CancelBatchRunCommand.Execute(null);
    await ViewModel.RunThisScriptCommand.ExecuteAsync(null);
    Assert.That(!ViewModel.CanCancelBatchRun);
    Assert.That(ViewModel.Log, Does.Contain(
      @"The batch run has been cancelled."));
  }

  [Test]
  public async Task SaveThisScript() {
    await ConfigureThisScript();
    await ViewModel.SaveThisScriptCommand.ExecuteAsync(null);
    Assert.That(ViewModel.TestBatchScript, Is.Not.Null);
    Assert.That(ViewModel.TestBatchScript.MockSerialiser.LastOutputPath, 
      Is.EqualTo(BatchScriptPath));
  }

  [Test]
  public async Task SaveThisScriptCancelled() {
    await ConfigureThisScript();
    MockDialogService.Cancel = true;
    await ViewModel.SaveThisScriptCommand.ExecuteAsync(null);
    Assert.That(ViewModel.TestBatchScript, Is.Null);
  }

  [Test]
  public async Task TemplateProgramsFolderNotFound() {
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.ProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.OriginalProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.OriginalProgramsFolder.Path);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.Contain(
      "Batch scripts cannot be run: cannot find template programs folder '"));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task UpdateScope() {
    ConfigureValidMockFileSystemService();
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
    Assert.That(ViewModel.Settings.Batch.SoundBank, Is.EqualTo(soundBank));
    Assert.That(ViewModel.Settings.Batch.Category, Is.EqualTo(category));
    Assert.That(ViewModel.Settings.Batch.Program, Is.EqualTo(program));
  }

  [Test]
  public async Task UpdateTasks() {
    ConfigureValidMockFileSystemService();
    await ViewModel.Open();
    Assert.That(ViewModel.Settings.Batch.Tasks, Has.Count.EqualTo(2));
    var taskItem = ViewModel.Tasks[0];
    Assert.That(taskItem.Name, Is.EqualTo("InitialiseLayout"));
    const string task = "PrependPathLineToDescription";
    taskItem.Name = task;
    await ViewModel.QueryClose();
    Assert.That(ViewModel.Settings.Batch.Tasks[0], Is.EqualTo(task));
  }

  [Test]
  public async Task ValidSettingsOnOpen() {
    ConfigureValidMockFileSystemService();
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(0));
  }

  private void AddSoundBankSubfolders(string folderPath) {
    TestHelper.AddSoundBankSubfolders(MockFileSystemService.Folder, folderPath);
  }

  private async Task ConfigureThisScript() {
    ConfigureValidMockFileSystemService();
    await ViewModel.Open();
    const string soundBank = "Factory";
    const string category = "Keys";
    const string program = "Morning Keys";
    ViewModel.Scope.SoundBank = soundBank;
    ViewModel.Scope.Category = category;
    ViewModel.Scope.Program = program;
  }

  private void ConfigureValidMockFileSystemService() {
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.ProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.OriginalProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.OriginalProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.TemplateProgramsFolder.Path);
    AddSoundBankSubfolders(Settings.TemplateProgramsFolder.Path);
    MockFileSystemService.Folder.ExpectedFilePaths.Add(
      Path.Combine(Settings.ProgramsFolder.Path, "Pulsar", "Plucks"), [
        "Lighthouse.uvip", "Music Box.uvip", "Resonator.uvip"
      ]);
    MockFileSystemService.Folder.ExpectedFilePaths.Add(
      Path.Combine(Settings.ProgramsFolder.Path, "Factory", "Keys"), [
        "Ballad Plucker.uvip", "Eighty Nine.uvip", "Morning Keys.uvip"
      ]);
  }
}