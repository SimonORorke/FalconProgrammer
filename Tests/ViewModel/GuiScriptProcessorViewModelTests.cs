using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class GuiScriptProcessorViewModelTests : ViewModelTestsBase {
  // Why in this test fixture but not others does code cleanup insist on moving
  // properties to before SetUp?
  private GuiScriptProcessorViewModel ViewModel { get; set; } = null!;

  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new GuiScriptProcessorViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  [Test]
  public async Task CutAndPaste() {
    MockSettingsReaderEmbedded.TestDeserialiser.EmbeddedFileName =
      "LocationsSettings.xml";
    var settings = MockSettingsReaderEmbedded.Read();
    ConfigureMockFileSystemService(settings);
    var settingsCategories =
      settings.MustUseGuiScriptProcessorCategories;
    int initialSettingsCategoriesCount = settingsCategories.Count;
    // Check that the test data is as expected
    Assert.That(initialSettingsCategoriesCount, Is.EqualTo(4));
    var initialFirstSettingsCategory = settingsCategories[0];
    var initialLastSettingsCategory = settingsCategories[3];
    // Populate
    await ViewModel.Open(); // Reads settings to populate the page.
    var collection = ViewModel.SoundBankCategories;
    int initialCategoriesCount = collection.Count;
    Assert.That(initialCategoriesCount, Is.EqualTo(initialSettingsCategoriesCount + 1));
    // Cut
    collection[3].CutCommand.Execute(null); // Last before addition item
    Assert.That(collection, Has.Count.EqualTo(initialCategoriesCount - 1));
    // Paste
    collection[0].PasteBeforeCommand.Execute(null);
    Assert.That(collection, Has.Count.EqualTo(initialCategoriesCount));
    // Update settings
    await ViewModel.QueryCloseAsync(); // Updates and saves settings
    settingsCategories = ViewModel.Settings.MustUseGuiScriptProcessorCategories;
    Assert.That(settingsCategories, Has.Count.EqualTo(initialSettingsCategoriesCount));
    Assert.That(settingsCategories[0].SoundBank,
      Is.EqualTo(initialLastSettingsCategory.SoundBank));
    Assert.That(settingsCategories[0].Category,
      Is.EqualTo(initialLastSettingsCategory.Category));
    Assert.That(settingsCategories[1].SoundBank,
      Is.EqualTo(initialFirstSettingsCategory.SoundBank));
    Assert.That(settingsCategories[1].Category,
      Is.EqualTo(initialFirstSettingsCategory.Category));
  }

  [Test]
  public async Task Main() {
    MockSettingsReaderEmbedded.TestDeserialiser.EmbeddedFileName =
      "LocationsSettings.xml";
    var settings = MockSettingsReaderEmbedded.Read();
    ConfigureMockFileSystemService(settings);
    await ViewModel.Open(); // Reads settings to populate the page.
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
    Assert.That(ViewModel.SoundBankCategories[0].RemoveCommand.CanExecute(
      null), Is.True);
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
    Assert.That(ViewModel.SoundBankCategories[4].RemoveCommand.CanExecute(
      null), Is.False);
    Assert.That(ViewModel.SoundBankCategories[4].IsAdditionItem, Is.True);
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.False);
    // Add item
    ViewModel.SoundBankCategories[4].SoundBank = "Spectre";
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.True);
    Assert.That(ViewModel.SoundBankCategories, Has.Count.EqualTo(6));
    Assert.That(ViewModel.SoundBankCategories[5].IsAdditionItem, Is.True);
    // Reset to test that sound bank change will be detected
    await ViewModel.Open();
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.False);
    // Change sound bank
    ViewModel.SoundBankCategories[0].SoundBank = "Spectre";
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.True);
    Assert.That(ViewModel.SoundBankCategories[0].Category, Is.EqualTo("All"));
    // Reset to test that category change will be detected
    await ViewModel.Open();
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.False);
    // Change category
    ViewModel.SoundBankCategories[0].Category = "Keys";
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.True);
    // Reset to test that item removal will be detected
    await ViewModel.Open();
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.False);
    ViewModel.SoundBankCategories[3].RemoveCommand.Execute(null);
    Assert.That(ViewModel.SoundBankCategories.HasBeenChanged, Is.True);
    Assert.That(ViewModel.SoundBankCategories, Has.Count.EqualTo(4));
    Assert.That(ViewModel.SoundBankCategories[3].IsAdditionItem, Is.True);
    await ViewModel.QueryCloseAsync(); // Updates and saves settings
    Assert.That(
      ViewModel.Settings.MustUseGuiScriptProcessorCategories, Has.Count.EqualTo(3));
    Assert.That(ViewModel.Settings.MustUseGuiScriptProcessorCategories[0].Category,
      Is.EqualTo("Organic Texture 2.8"));
    // Check that Category 'All' on the page has ben saved as empty in Settings. 
    Assert.That(
      ViewModel.Settings.MustUseGuiScriptProcessorCategories[1].Category, Is.Empty);
  }

  [Test]
  public async Task NoProgramsFolder() {
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Is.EqualTo(
      "Script processors cannot be updated: the programs folder has not been specified."));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task ProgramsFolderEmpty() {
    MockSettingsReaderEmbedded.TestDeserialiser.EmbeddedFileName =
      "LocationsSettings.xml";
    var settings = MockSettingsReaderEmbedded.Read();
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(settings.ProgramsFolder.Path,
      []);
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.EndWith(
      "' contains no sound bank subfolders."));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task ProgramsFolderNotFound() {
    MockSettingsReaderEmbedded.TestDeserialiser.EmbeddedFileName =
      "LocationsSettings.xml";
    MockFileSystemService.Folder.ExpectedExists = false;
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Script processors cannot be updated: cannot find programs folder "));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  private void ConfigureMockFileSystemService(Settings settings) {
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      settings.ProgramsFolder.Path, [
        "Ether Fields", "Factory", "Organic Keys", "Pulsar", "Spectre", "Voklm"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Ether Fields"), [
        "Granular", "Hybrid"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Factory"), [
        "Bass-Sub", "Keys", "Organic Texture 2.8"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Organic Keys"), [
        "Acoustic Mood", "Lo-Fi"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Pulsar"), [
        "Bass", "Leads"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Spectre"), [
        "Bells", "Chords"
      ]);
    MockFileSystemService.Folder.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Voklm"), [
        "Synth Choirs", "Vox Instruments"
      ]);
  }
}