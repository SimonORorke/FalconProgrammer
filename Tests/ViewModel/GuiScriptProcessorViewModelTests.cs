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
    ViewModel = new GuiScriptProcessorViewModel {
      View = MockView,
      ServiceHelper = ServiceHelper
    };
  }

  [Test]
  public void Main() {
    TestSettingsReader.TestDeserialiser.EmbeddedResourceFileName =
      "LocationsSettings.xml";
    var settings = TestSettingsReader.Read();
    MockFileSystemService.ExpectedSubfolderNames.Add(
      settings.ProgramsFolder.Path, SoundBanks);
    MockFileSystemService.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Ether Fields"), EtherFieldsCategories);
    MockFileSystemService.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Factory"), FactoryCategories);
    MockFileSystemService.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Organic Keys"), OrganicKeysCategories);
    MockFileSystemService.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Pulsar"), PulsarCategories);
    MockFileSystemService.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Spectre"), SpectreCategories);
    MockFileSystemService.ExpectedSubfolderNames.Add(
      Path.Combine(settings.ProgramsFolder.Path, "Voklm"), VoklmCategories);
    ViewModel.OnAppearing();
    Assert.That(MockView.DispatchCount, Is.EqualTo(1));
    Assert.That(MockAlertService.ShowAlertCount, Is.EqualTo(0));
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
  }

  [Test]
  public void NoProgramsFolder() {
    ViewModel.OnAppearing();
    Assert.That(MockAlertService.ShowAlertCount, Is.EqualTo(1));
    Assert.That(MockAlertService.LastMessage, Is.EqualTo(
      "Script processors cannot be updated: a programs folder has not been specified."));
    Assert.That(MockView.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public void ProgramsFolderEmpty() {
    TestSettingsReader.TestDeserialiser.EmbeddedResourceFileName =
      "LocationsSettings.xml";
    var settings = TestSettingsReader.Read();
    MockFileSystemService.ExpectedSubfolderNames.Add(settings.ProgramsFolder.Path, []);
    ViewModel.OnAppearing();
    Assert.That(MockAlertService.ShowAlertCount, Is.EqualTo(1));
    Assert.That(MockAlertService.LastMessage, Does.EndWith(
      "' contains no sound bank subfolders."));
    Assert.That(MockView.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public void ProgramsFolderNotFound() {
    TestSettingsReader.TestDeserialiser.EmbeddedResourceFileName =
      "LocationsSettings.xml";
    MockFileSystemService.ExpectedFolderExists = false;
    ViewModel.OnAppearing();
    Assert.That(MockAlertService.ShowAlertCount, Is.EqualTo(1));
    Assert.That(MockAlertService.LastMessage, Does.StartWith(
      "Script processors cannot be updated: cannot find programs folder "));
    Assert.That(MockView.GoToLocationsPageCount, Is.EqualTo(1));
  }
}