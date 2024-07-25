using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class BackgroundCollectionTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = ReadMockSettings("LocationsSettings.xml");
    Collection = new BackgroundCollection(MockDialogService,
      MockFileSystemService, MockDispatcherService);
  }

  private BackgroundCollection Collection { get; set; } = null!;
  private Settings Settings { get; set; } = null!;

  private static IEnumerable<string> SoundBanks =>
    ["Eternal Funk", "Fluidity", "Hypnotic Drive", "Inner Dimensions"];

  [Test]
  public async Task Browse() {
    Collection.Populate(Settings, SoundBanks);
    MockDialogService.SimulatedPath =
      @"K:\NewLeaf\Background Images\Dark Forest.png";
    var command = (AsyncRelayCommand)Collection[0].BrowseCommand;
    await command.ExecuteAsync(null);
    Assert.That(Collection[0].Path,
      Is.EqualTo(MockDialogService.SimulatedPath));
  }

  [Test]
  public async Task BrowseCancelled() {
    Collection.Populate(Settings, SoundBanks);
    MockDialogService.SimulatedPath =
      @"K:\NewLeaf\Background Images\Dark Forest.png";
    MockDialogService.Cancel = true;
    var command = (AsyncRelayCommand)Collection[0].BrowseCommand;
    await command.ExecuteAsync(null);
    Assert.That(Collection[0].Path,
      Is.Not.EqualTo(MockDialogService.SimulatedPath));
  }

  [Test]
  public void Main() {
    int initialSettingsBackgroundCount = Settings.Backgrounds.Count;
    // Check that the test data is as expected
    Assert.That(initialSettingsBackgroundCount, Is.EqualTo(2));
    // Populate
    Collection.Populate(Settings, SoundBanks);
    int initialCollectionCount = Collection.Count;
    Assert.That(initialCollectionCount, Is.EqualTo(initialSettingsBackgroundCount + 1));
    Assert.That(Collection[0].SoundBanks, Has.Count.EqualTo(4));
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
    Assert.That(Settings.Backgrounds, Has.Count.EqualTo(
      initialSettingsBackgroundCount - 1));
  }
}