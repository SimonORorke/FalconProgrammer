using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MidiForMacrosViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new MidiForMacrosViewModel(MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
    Task.Run(async () => await ViewModel.Open()).Wait();
  }

  private MidiForMacrosViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task DisallowOverlappingContinuousCcNoRange() {
    // Check that data is as expected.
    var rangesInSettings =
      ViewModel.Settings.MidiForMacros.ContinuousCcNoRanges;
    Assert.That(rangesInSettings[0].Start, Is.EqualTo(31));
    Assert.That(rangesInSettings[0].End, Is.EqualTo(34));
    await DisallowOverlappingCcNoRange(
      32, 35, ViewModel.ContinuousCcNoRanges);
  }

  [Test]
  public async Task DisallowOverlappingToggleCcNoRange() {
    // Check that data is as expected.
    var rangesInSettings =
      ViewModel.Settings.MidiForMacros.ToggleCcNoRanges;
    Assert.That(rangesInSettings[0].Start, Is.EqualTo(112));
    Assert.That(rangesInSettings[0].End, Is.EqualTo(112));
    await DisallowOverlappingCcNoRange(
      112, 113, ViewModel.ToggleCcNoRanges);
  }

  [Test]
  public async Task UpdateContinuousCcNoRanges() {
    await UpdateCcNoRanges(
      ViewModel.ContinuousCcNoRanges,
      ViewModel.Settings.MidiForMacros.ContinuousCcNoRanges);
  }

  [Test]
  public async Task UpdateModWheelReplacementCcNo() {
    const int newCcNo = 18;
    ViewModel.ModWheelReplacementCcNo = newCcNo;
    Assert.That(await ViewModel.QueryCloseAsync(), Is.True);
    Assert.That(ViewModel.Settings.MidiForMacros.ModWheelReplacementCcNo,
      Is.EqualTo(newCcNo));
  }

  [Test]
  public async Task UpdateToggleCcNoRanges() {
    await UpdateCcNoRanges(
      ViewModel.ToggleCcNoRanges,
      ViewModel.Settings.MidiForMacros.ToggleCcNoRanges);
  }

  [Test]
  public void ValidateModWheelReplacementCcNo() {
    const int invalidCcNo = 128;
    ViewModel.ModWheelReplacementCcNo = invalidCcNo;
    var errors = ViewModel.GetErrors().ToList();
    Assert.That(errors, Has.Count.EqualTo(1));
    var memberNames = errors[0].MemberNames.ToList();
    Assert.That(memberNames, Has.Count.EqualTo(1));
    Assert.That(memberNames[0], Is.EqualTo(nameof(ViewModel.ModWheelReplacementCcNo)));
    Assert.That(ViewModel.ModWheelReplacementCcNo, Is.EqualTo(invalidCcNo));
  }

  private async Task DisallowOverlappingCcNoRange(
    int newRangeStart, int newRangeEnd, CcNoRangeCollection ranges) {
    // Add a new range.
    var newRange = TestHelper.CreateCcNoRangeAdditionItem(newRangeStart, newRangeEnd);
    ranges.Add(newRange);
    // Update settings.
    Assert.That(!await ViewModel.QueryCloseAsync(true));
  }

  private async Task UpdateCcNoRanges(
    CcNoRangeCollection ranges, List<Settings.IntegerRange> rangesInSettings) {
    // Add a new range.
    var newRange = TestHelper.CreateCcNoRangeAdditionItem(21, 24);
    ranges.Add(newRange);
    // Update settings.
    Assert.That(await ViewModel.QueryCloseAsync());
    Assert.That(rangesInSettings[^1].Start == newRange.Start
                && rangesInSettings[^1].End == newRange.End);
  }
}