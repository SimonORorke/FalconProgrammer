using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class GuiScriptProcessorViewModelTests : ViewModelTestsBase {
  private static readonly string[] EtherFieldsCategories = [
    "Granular", "Hybrid"
  ];

  private static readonly string[] FactoryCategories = [
    "Bass-Sub", "Keys", "Organic Texture 2.8"
  ];

  private static readonly string[] OrganicKeysCategories = [
    "Acoustic Mood", "Lo-Fi"
  ];

  private static readonly string[] PulsarCategories = [
    "Bass", "Leads"
  ];

  private static readonly string[] SoundBanks = [
    "Ether Fields", "Factory", "Organic Keys", "Pulsar", "Spectre", "Voklm"
  ];

  private static readonly string[] SpectreCategories = [
    "Bells", "Chords"
  ];

  private static readonly string[] VoklmCategories = [
    "Synth Choirs", "Vox Instruments"
  ];

  private GuiScriptProcessorViewModel ViewModel { get; set; } = null!;

  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new GuiScriptProcessorViewModel(MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  [Test]
  public void Main() {
    TestSettingsReaderEmbedded.TestDeserialiser.EmbeddedResourceFileName =
      "LocationsSettings.xml";
    var settings = TestSettingsReaderEmbedded.Read();
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      settings.ProgramsFolder.Path, SoundBanks);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Ether Fields"), EtherFieldsCategories);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Factory"), FactoryCategories);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Organic Keys"), OrganicKeysCategories);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Pulsar"), PulsarCategories);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Spectre"), SpectreCategories);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Voklm"), VoklmCategories);
    ViewModel.Open(); // Reads settings to populate the page.
    // Assert.That(MockDispatcherService.DispatchCount, Is.EqualTo(1));
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(0));
    Assert.That(ViewModel.SoundBankCategories, Has.Count.EqualTo(5));
    Assert.That(ViewModel.SoundBankCategories[0].SoundBank, Is.EqualTo("Factory"));
    Assert.That(ViewModel.SoundBankCategories[0].Category,
      Is.EqualTo("Organic Texture 2.8"));
    Assert.That(ViewModel.SoundBankCategories[0].SoundBanks, Has.Count.EqualTo(6));
    Assert.That(ViewModel.SoundBankCategories[0].Categories, Has.Count.EqualTo(4));
    Assert.That(ViewModel.SoundBankCategories[0].Categories[0], Is.EqualTo("All"));
    Assert.That(ViewModel.SoundBankCategories[0].Categories[1], Is.EqualTo("Bass-Sub"));
    Assert.That(ViewModel.SoundBankCategories[0].CanRemove, Is.True);
    Assert.That(ViewModel.SoundBankCategories[0].IsAdditionItem, Is.False);
    Assert.That(ViewModel.SoundBankCategories[0].IsForAllCategories, Is.False);
    Assert.That(ViewModel.SoundBankCategories[1].SoundBank, Is.EqualTo("Organic Keys"));
    Assert.That(ViewModel.SoundBankCategories[1].Category, Is.EqualTo("All"));
    Assert.That(ViewModel.SoundBankCategories[1].IsForAllCategories, Is.True);
    // Addition item
    Assert.That(ViewModel.SoundBankCategories[4].SoundBank, Is.Empty);
    Assert.That(ViewModel.SoundBankCategories[4].SoundBanks, Has.Count.EqualTo(6));
    Assert.That(ViewModel.SoundBankCategories[4].Categories, Has.Count.EqualTo(0));
    Assert.That(ViewModel.SoundBankCategories[4].Category, Is.Empty);
    Assert.That(ViewModel.SoundBankCategories[4].CanRemove, Is.False);
    Assert.That(ViewModel.SoundBankCategories[4].IsAdditionItem, Is.True);
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.False);
    // Add item
    ViewModel.SoundBankCategories[4].SoundBank = "Spectre";
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.True);
    Assert.That(ViewModel.SoundBankCategories, Has.Count.EqualTo(6));
    Assert.That(ViewModel.SoundBankCategories[5].IsAdditionItem, Is.True);
    // Reset to test that sound bank change will be detected
    ViewModel.Open();
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.False);
    // Change sound bank
    ViewModel.SoundBankCategories[0].SoundBank = "Spectre";
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.True);
    Assert.That(ViewModel.SoundBankCategories[0].Category, Is.EqualTo("All"));
    // Reset to test that category change will be detected
    ViewModel.Open();
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.False);
    // Change category
    ViewModel.SoundBankCategories[0].Category = "Keys";
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.True);
    // Reset to test that item removal will be detected
    ViewModel.Open();
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.False);
    ViewModel.SoundBankCategories[3].RemoveCommand.Execute(null);
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.True);
    Assert.That(ViewModel.SoundBankCategories, Has.Count.EqualTo(4));
    Assert.That(ViewModel.SoundBankCategories[3].IsAdditionItem, Is.True);
    ViewModel.QueryClose(); // Updates and saves settings
    Assert.That(
      ViewModel.Settings.MustUseGuiScriptProcessorCategories, Has.Count.EqualTo(3));
    Assert.That(ViewModel.Settings.MustUseGuiScriptProcessorCategories[0].Category,
      Is.EqualTo("Organic Texture 2.8"));
    // Check that Category 'All' on the page has ben saved as empty in Settings. 
    Assert.That(
      ViewModel.Settings.MustUseGuiScriptProcessorCategories[1].Category, Is.Empty);
  }

  [Test]
  public void NoProgramsFolder() {
    ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Is.EqualTo(
      "Script processors cannot be updated: a programs folder has not been specified."));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public void ProgramsFolderEmpty() {
    TestSettingsReaderEmbedded.TestDeserialiser.EmbeddedResourceFileName =
      "LocationsSettings.xml";
    var settings = TestSettingsReaderEmbedded.Read();
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(settings.ProgramsFolder.Path, []);
    ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.EndWith(
      "' contains no sound bank subfolders."));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public void ProgramsFolderNotFound() {
    TestSettingsReaderEmbedded.TestDeserialiser.EmbeddedResourceFileName =
      "LocationsSettings.xml";
    MockFileSystemService.Folder.ExpectedExists = false;
    ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Script processors cannot be updated: cannot find programs folder "));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }
}