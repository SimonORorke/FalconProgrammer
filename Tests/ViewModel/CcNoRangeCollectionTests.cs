using System.Collections;
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
  public async Task DisallowOverlappingContinuousCcNoRange() {
    await DisallowOverlappingRange<ContinuousCcNoRangeCollection>();
  }

  [Test]
  public void PopulateContinuousCcNoRanges() {
    Populate<ContinuousCcNoRangeCollection>(Settings.MidiForMacros.ContinuousCcNoRanges);
  }

  [Test]
  public void PopulateToggleCcNoRanges() {
    Populate<ToggleCcNoRangeCollection>(Settings.MidiForMacros.ToggleCcNoRanges);
  }

  [Test]
  public async Task UpdateContinuousCcNoRanges() {
    await Update<ContinuousCcNoRangeCollection>(Settings.MidiForMacros.ContinuousCcNoRanges);
  }

  [Test]
  public async Task UpdateToggleCcNoRanges() {
    // Check that there is only one, as we want to test removing the last one.
    Assert.That(Settings.MidiForMacros.ToggleCcNoRanges, Has.Count.EqualTo(1));
    await Update<ToggleCcNoRangeCollection>(Settings.MidiForMacros.ToggleCcNoRanges);
  }

  private async Task DisallowOverlappingRange<T>()
    where T : CcNoRangeCollection {
    var ranges = CreateRanges<T>();
    ranges.Populate(Settings);
    var lastRange = ranges[^1];
    var overlappingRange = TestHelper.CreateCcNoRangeAdditionItem(
      lastRange.Start, lastRange.End + 1);
    ranges.Add(overlappingRange);
    var updateResult = await ranges.UpdateSettingsAsync(false);
    Assert.That(!updateResult.Success);
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
  }

  private void Populate<T>(ICollection rangesInSettings)
    where T : CcNoRangeCollection {
    var ranges = CreateRanges<T>();
    ranges.Populate(Settings);
    // Should include addition item.
    Assert.That(ranges, Has.Count.EqualTo(
      rangesInSettings.Count + 1));
  }

  private async Task Update<T>(List<Settings.IntegerRange> rangesInSettings)
    where T : CcNoRangeCollection {
    int originalCountInSettings = rangesInSettings.Count;
    var ranges = CreateRanges<T>();
    ranges.Populate(Settings);
    ranges[0].RemoveCommand.Execute(null);
    await ranges.UpdateSettingsAsync(false);
    Assert.That(rangesInSettings, Has.Count.EqualTo(
      originalCountInSettings - 1));
  }

  private CcNoRangeCollection CreateRanges<T>()
    where T : CcNoRangeCollection {
    return (T)Activator.CreateInstance(typeof(T),
      [MockDialogService, MockDispatcherService])!;
  }
}