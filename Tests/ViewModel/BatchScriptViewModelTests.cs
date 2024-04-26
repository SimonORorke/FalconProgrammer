using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class BatchScriptViewModelTests : ViewModelTestsBase {
  private BatchScriptViewModel ViewModel { get; set; } = null!;

  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new BatchScriptViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  [Test]
  public async Task DefaultTemplateFileNotFound() {
    MockSettingsReaderEmbedded.TestDeserialiser.EmbeddedFileName =
      "LocationsSettings.xml";
    var settings = MockSettingsReaderEmbedded.Read();
    MockFileSystemService.Folder.ExistingPaths.Add(settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(settings.ProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(settings.OriginalProgramsFolder.Path);
    AddSoundBankSubfolders(settings.OriginalProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(settings.TemplateProgramsFolder.Path);
    AddSoundBankSubfolders(settings.TemplateProgramsFolder.Path);
    MockFileSystemService.File.ExistingPaths.Add(
      @"J:\FalconProgrammer\Settings\Settings.xml");
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Batch scripts cannot be run: cannot find default template file '"));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task OriginalProgramsFolderNotFound() {
    MockSettingsReaderEmbedded.TestDeserialiser.EmbeddedFileName =
      "LocationsSettings.xml";
    var settings = MockSettingsReaderEmbedded.Read();
    MockFileSystemService.Folder.ExistingPaths.Add(settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(settings.ProgramsFolder.Path);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Batch scripts cannot be run: cannot find original programs folder '"));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task NoDefaultTemplateFile() {
    MockSettingsReaderEmbedded.TestDeserialiser.EmbeddedFileName =
      "NoDefaultTemplate.xml";
    var settings = MockSettingsReaderEmbedded.Read();
    MockFileSystemService.Folder.ExistingPaths.Add(settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(settings.ProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(settings.OriginalProgramsFolder.Path);
    AddSoundBankSubfolders(settings.OriginalProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(settings.TemplateProgramsFolder.Path);
    AddSoundBankSubfolders(settings.TemplateProgramsFolder.Path);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Is.EqualTo(
      "Batch scripts cannot be run: the default template file has not been specified."));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task NoProgramsFolder() {
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Is.EqualTo(
      "Batch scripts cannot be run: the programs folder has not been specified."));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task TemplateProgramsFolderNotFound() {
    MockSettingsReaderEmbedded.TestDeserialiser.EmbeddedFileName =
      "LocationsSettings.xml";
    var settings = MockSettingsReaderEmbedded.Read();
    MockFileSystemService.Folder.ExistingPaths.Add(settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(settings.ProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(settings.OriginalProgramsFolder.Path);
    AddSoundBankSubfolders(settings.OriginalProgramsFolder.Path);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Batch scripts cannot be run: cannot find template programs folder '"));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task ValidSettingsOnOpen() {
    MockSettingsReaderEmbedded.TestDeserialiser.EmbeddedFileName =
      "LocationsSettings.xml";
    var settings = MockSettingsReaderEmbedded.Read();
    MockFileSystemService.Folder.ExistingPaths.Add(settings.ProgramsFolder.Path);
    AddSoundBankSubfolders(settings.ProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(settings.OriginalProgramsFolder.Path);
    AddSoundBankSubfolders(settings.OriginalProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(settings.TemplateProgramsFolder.Path);
    AddSoundBankSubfolders(settings.TemplateProgramsFolder.Path);
    MockFileSystemService.File.ExpectedExists = true;
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(0));
  }

  private void AddSoundBankSubfolders(string folderPath) {
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      folderPath, [
        "Ether Fields", "Factory", "Organic Keys", "Pulsar", "Spectre", "Voklm"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Ether Fields"), [
        "Granular", "Hybrid"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Factory"), [
        "Bass-Sub", "Keys", "Organic Texture 2.8"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Organic Keys"), [
        "Acoustic Mood", "Lo-Fi"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Pulsar"), [
        "Bass", "Leads"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Spectre"), [
        "Bells", "Chords"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Voklm"), [
        "Synth Choirs", "Vox Instruments"
      ]);
  }
}