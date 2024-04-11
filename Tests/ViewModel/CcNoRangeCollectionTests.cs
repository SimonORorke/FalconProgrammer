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
  public void PopulateContinuousCcNoRanges() {
    Populate<ContinuousCcNoRangeCollection>(Settings.MidiForMacros.ContinuousCcNoRanges);
  }

  [Test]
  public void PopulateToggleCcNoRanges() {
    Populate<ToggleCcNoRangeCollection>(Settings.MidiForMacros.ToggleCcNoRanges);
  }

  [Test]
  public void UpdateContinuousCcNoRanges() {
    Update<ContinuousCcNoRangeCollection>(Settings.MidiForMacros.ContinuousCcNoRanges);
  }

  [Test]
  public void UpdateToggleCcNoRanges() {
    Update<ToggleCcNoRangeCollection>(Settings.MidiForMacros.ToggleCcNoRanges);
  }

  private void Populate<T>(ICollection rangesInSettings)
    where T : CcNoRangeCollection {
    var ranges = CreateRanges<T>();
    ranges.Populate(Settings);
    // Should include addition item.
    Assert.That(ranges, Has.Count.EqualTo(
      rangesInSettings.Count + 1));
  }

  private void Update<T>(List<Settings.IntegerRange> rangesInSettings)
    where T : CcNoRangeCollection {
    int originalCountInSettings = rangesInSettings.Count;
    var ranges = CreateRanges<T>();
    ranges.Populate(Settings);
    ranges[0].RemoveCommand.Execute(null);
    ranges.UpdateSettings();
    Assert.That(rangesInSettings, Has.Count.EqualTo(
      originalCountInSettings - 1));
  }

  private CcNoRangeCollection CreateRanges<T>() where T : CcNoRangeCollection {
    return (T)Activator.CreateInstance(typeof(T), [MockDispatcherService])!;
  }
}