using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class DoNotZeroReverbCollectionTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = ReadMockSettings("BatchSettings.xml");
    Collection = new DoNotZeroReverbCollection(MockDialogService,
      MockFileSystemService, MockDispatcherService);
  }

  private DoNotZeroReverbCollection Collection { get; set; } = null!;
  private Settings Settings { get; set; } = null!;

  private static IEnumerable<string> SoundBanks => [
    "Eternal Funk", "Ether Fields", "Falcon Factory", "Fluidity", "Hypnotic Drive", 
    "Inner Dimensions", "Savage", "Spectre"
  ];

  [Test]
  public async Task InvalidProgramItem() {
    ConfigureMockFileSystemService();
    Collection.Populate(Settings, SoundBanks);
    Assert.That(Collection.HasBeenChanged, Is.False);
    Collection[0].CutCommand.Execute(null);
    Assert.That(Collection[0].CutCommand.CanExecute(null), Is.True);
    // Add an incomplete program item.
    const string soundBank = "Ether Fields";
    Collection[^1].SoundBank = soundBank; // Addition item
    // No longer addition item
    Assert.That(Collection.HasBeenChanged, Is.True);
    Assert.That(Collection[^2].IsAdditionItem, Is.False);
    Assert.That(Collection[^2].SoundBank, Is.EqualTo(soundBank));
    Assert.That(Collection[^2].CutCommand.CanExecute(null), Is.True);
    Assert.That(Collection[^2].PasteBeforeCommand.CanExecute(null), Is.True);
    Assert.That(Collection[^2].RemoveCommand.CanExecute(null), Is.True);
    MockDialogService.SimulatedYesNoAnswer = true;
    var closingValidationResult = await Collection.Validate(true);
    Assert.That(closingValidationResult.Success, Is.False);
    Assert.That(closingValidationResult.CanClosePage, Is.True);
    Assert.That(MockDialogService.LastYesNoQuestion, Does.StartWith(
      "Sound Bank, Category and Program must all be specified. "));
  }

  [Test]
  public async Task Main() {
    ConfigureMockFileSystemService();
    int initialSettingsDoNotZeroReverbCount = Settings.DoNotZeroReverb.Count;
    // Check that the test data is as expected
    Assert.That(initialSettingsDoNotZeroReverbCount, Is.EqualTo(13));
    // Populate
    Collection.Populate(Settings, SoundBanks);
    int initialCollectionCount = Collection.Count;
    Assert.That(initialCollectionCount,
      Is.EqualTo(initialSettingsDoNotZeroReverbCount + 1));
    Assert.That(Collection[0].SoundBanks, Has.Count.EqualTo(SoundBanks.Count()));
    // Cut
    Collection[^2].CutCommand.Execute(null); // Last before addition item
    Assert.That(Collection, Has.Count.EqualTo(initialCollectionCount - 1));
    // Paste
    Collection[0].PasteBeforeCommand.Execute(null);
    Assert.That(Collection, Has.Count.EqualTo(initialCollectionCount));
    // Remove
    Collection[0].RemoveCommand.Execute(null);
    Assert.That(Collection, Has.Count.EqualTo(initialCollectionCount - 1));
    // Validate
    var closingValidationResult = await Collection.Validate(true);
    Assert.That(closingValidationResult.Success, Is.True);
    // Update Settings
    Collection.UpdateSettings();
    Assert.That(Settings.DoNotZeroReverb, Has.Count.EqualTo(
      initialSettingsDoNotZeroReverbCount - 1));
  }

  private void ConfigureMockFileSystemService() {
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.ProgramsFolder.Path);
    TestHelper.AddSoundBankSubfolders(
      MockFileSystemService.Folder, Settings.ProgramsFolder.Path);
  }
}