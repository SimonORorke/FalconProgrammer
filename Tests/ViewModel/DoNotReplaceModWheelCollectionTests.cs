using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class DoNotReplaceModWheelCollectionTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = ReadMockSettings("BatchSettings.xml");
    Collection = new DoNotReplaceModWheelCollection(
      MockFileSystemService, MockDispatcherService);
  }

  private DoNotReplaceModWheelCollection Collection { get; set; } = null!;
  private Settings Settings { get; set; } = null!;

  private static IEnumerable<string> SoundBanks =>
    ["Ether Fields", "Fluidity", "Hypnotic Drive", "Inner Dimensions"];

  [Test]
  public void Main() {
    int initialSettingsSoundBankCount = 
      Settings.MidiForMacros.DoNotReplaceModWheelWithMacroSoundBanks.Count;
    // Check that the test data is as expected
    Assert.That(initialSettingsSoundBankCount, Is.EqualTo(1));
    // Populate
    Collection.Populate(Settings, SoundBanks);
    int initialCollectionCount = Collection.Count;
    Assert.That(initialCollectionCount, Is.EqualTo(initialSettingsSoundBankCount + 1));
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
    Assert.That(Settings.MidiForMacros.DoNotReplaceModWheelWithMacroSoundBanks, 
      Has.Count.EqualTo(initialSettingsSoundBankCount - 1));
  }
}