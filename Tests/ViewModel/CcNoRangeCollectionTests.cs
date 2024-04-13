using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class CcNoRangeCollectionTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = TestSettingsReaderEmbedded.Read(true);
  }

  private Settings Settings { get; set; } = null!;

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

  private async Task Update(
    List<Settings.IntegerRange> settingsRanges, CcNoRangeCollection ranges) {
    int originalCountInSettings = settingsRanges.Count;
    ranges.Populate(settingsRanges);
    ranges[0].RemoveCommand.Execute(null);
    await ranges.UpdateSettingsAsync(false);
    Assert.That(settingsRanges, Has.Count.EqualTo(
      originalCountInSettings - 1));
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