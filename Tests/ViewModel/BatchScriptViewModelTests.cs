using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class BatchScriptViewModelTests : ViewModelTestsBase {
  private Settings Settings { get; set; } = null!;
  private BatchScriptViewModel ViewModel { get; set; } = null!;

  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel = new BatchScriptViewModel(
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
    var scope = ViewModel.Scopes[0];
    Assert.That(scope.SoundBank, Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(scope.SoundBanks[0], Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(scope.Category, Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(scope.Categories[0], Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(scope.Program, Is.EqualTo(SoundBankItem.AllCaption));
    Assert.That(scope.Programs[0], Is.EqualTo(SoundBankItem.AllCaption));
    scope.SoundBank = soundBank;
    scope.Category = category;
    scope.Program = program;
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
    var taskViewModel = ViewModel.Tasks[0];
    Assert.That(taskViewModel.Task, Is.EqualTo("InitialiseLayout"));
    const string task = "PrependPathLineToDescription";
    taskViewModel.Task = task;
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
  }
}