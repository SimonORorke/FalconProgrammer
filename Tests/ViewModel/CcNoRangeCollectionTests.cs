using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class CcNoRangeCollectionTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = MockSettingsReaderEmbedded.Read();
  }

  private Settings Settings { get; set; } = null!;

  [Test]
  public async Task CutAndPaste() {
    var settingsRanges = Settings.MidiForMacros.ContinuousCcNoRanges;
    int initialSettingsRangesCount = settingsRanges.Count;
    // Check that the test data is as expected
    Assert.That(initialSettingsRangesCount, Is.EqualTo(7));
    var initialFirstSettingsRange = settingsRanges[0];
    var initialLastSettingsRange = settingsRanges[6];
    var ranges = CreateContinuousCcNoRanges();
    // Populate
    ranges.Populate(settingsRanges);
    int initialRangesCount = ranges.Count;
    Assert.That(initialRangesCount, Is.EqualTo(initialSettingsRangesCount + 1));
    Assert.That(ranges[0].CutCommand.CanExecute(null), Is.True);
    Assert.That(ranges[0].PasteBeforeCommand.CanExecute(null), Is.False);
    Assert.That(ranges[^1].CutCommand.CanExecute(null), Is.False); // Addition item
    Assert.That(ranges[^1].PasteBeforeCommand.CanExecute(null), Is.False);
    // Cut
    ranges[6].CutCommand.Execute(null); // Last before addition item
    Assert.That(ranges, Has.Count.EqualTo(initialRangesCount - 1));
    Assert.That(ranges[0].CutCommand.CanExecute(null), Is.True);
    Assert.That(ranges[0].PasteBeforeCommand.CanExecute(null), Is.True);
    Assert.That(ranges[^1].CutCommand.CanExecute(null), Is.False); // Addition item
    Assert.That(ranges[^1].PasteBeforeCommand.CanExecute(null), Is.True);
    // Paste
    ranges[0].PasteBeforeCommand.Execute(null);
    Assert.That(ranges, Has.Count.EqualTo(initialRangesCount));
    Assert.That(ranges[0].CutCommand.CanExecute(null), Is.True);
    Assert.That(ranges[0].PasteBeforeCommand.CanExecute(null), Is.False);
    Assert.That(ranges[^1].CutCommand.CanExecute(null), Is.False); // Addition item
    Assert.That(ranges[^1].PasteBeforeCommand.CanExecute(null), Is.False);
    // Update settings
    await ranges.UpdateSettingsAsync(false);
    Assert.That(settingsRanges, Has.Count.EqualTo(initialSettingsRangesCount));
    Assert.That(settingsRanges[0], Is.EqualTo(initialLastSettingsRange));
    Assert.That(settingsRanges[1], Is.EqualTo(initialFirstSettingsRange));
  }

  [Test]
  public async Task DisallowOverlappingRange() {
    var ranges = CreateContinuousCcNoRanges();
    ranges.Populate(Settings.MidiForMacros.ContinuousCcNoRanges);
    var lastRange = ranges[^1];
    var overlappingRange = TestHelper.CreateCcNoRangeAdditionItem(
      lastRange.Start, lastRange.End + 1);
    ranges.Add(overlappingRange);
    var updateResult = await ranges.UpdateSettingsAsync(false);
    Assert.That(!updateResult.Success);
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
  }

  [Test]
  public void Populate() {
    var ranges = CreateContinuousCcNoRanges();
    ranges.Populate(Settings.MidiForMacros.ContinuousCcNoRanges);
    // Should include addition item.
    Assert.That(ranges, Has.Count.EqualTo(
      Settings.MidiForMacros.ContinuousCcNoRanges.Count + 1));
  }

  [Test]
  public async Task UpdateContinuousCcNoRanges() {
    await Update(
      Settings.MidiForMacros.ContinuousCcNoRanges, CreateContinuousCcNoRanges());
  }

  [Test]
  public async Task UpdateToggleCcNoRanges() {
    // Check that there is only one, as we want to test removing the last one.
    Assert.That(Settings.MidiForMacros.ToggleCcNoRanges, Has.Count.EqualTo(1));
    await Update(Settings.MidiForMacros.ToggleCcNoRanges, CreateToggleCcNoRanges());
  }

  private static async Task Update(
    List<Settings.IntegerRange> settingsRanges, CcNoRangeCollection ranges) {
    int initialSettingsRangesCount = settingsRanges.Count;
    ranges.Populate(settingsRanges);
    ranges[0].RemoveCommand.Execute(null);
    await ranges.UpdateSettingsAsync(false);
    Assert.That(settingsRanges, Has.Count.EqualTo(
      initialSettingsRangesCount - 1));
  }

  private CcNoRangeCollection CreateContinuousCcNoRanges() {
    return new CcNoRangeCollection("Continuous",
      MockDialogService, MockDispatcherService);
  }

  private CcNoRangeCollection CreateToggleCcNoRanges() {
    return new CcNoRangeCollection("Toggle",
      MockDialogService, MockDispatcherService);
  }
}