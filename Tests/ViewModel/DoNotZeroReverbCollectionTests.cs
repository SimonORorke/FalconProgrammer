using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class DoNotZeroReverbCollectionTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = ReadMockSettings("BatchSettings.xml");
    Collection = new DoNotZeroReverbCollection(
      MockFileSystemService, MockDispatcherService);
  }

  private DoNotZeroReverbCollection Collection { get; set; } = null!;
  private Settings Settings { get; set; } = null!;

  private static IEnumerable<string> SoundBanks => [
    "Eternal Funk", "Factory", "Fluidity", "Hypnotic Drive", "Inner Dimensions",
    "Savage", "Spectre"
  ];

  [Test]
  public void Main() {
    ConfigureMockFileSystemService();
    int initialSettingsDoNotZeroReverbCount = Settings.DoNotZeroReverb.Count;
    // Check that the test data is as expected
    Assert.That(initialSettingsDoNotZeroReverbCount, Is.EqualTo(13));
    // Populate
    Collection.Populate(Settings, SoundBanks);
    int initialCollectionCount = Collection.Count;
    Assert.That(initialCollectionCount,
      Is.EqualTo(initialSettingsDoNotZeroReverbCount + 1));
    Assert.That(Collection[0].SoundBanks, Has.Count.EqualTo(7));
    // Cut
    Collection[^2].CutCommand.Execute(null); // Last before addition item
    Assert.That(Collection, Has.Count.EqualTo(initialCollectionCount - 1));
    // Paste
    Collection[0].PasteBeforeCommand.Execute(null);
    Assert.That(Collection, Has.Count.EqualTo(initialCollectionCount));
    // Remove
    Collection[0].RemoveCommand.Execute(null);
    Assert.That(Collection, Has.Count.EqualTo(initialCollectionCount - 1));
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