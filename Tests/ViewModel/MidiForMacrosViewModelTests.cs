using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MidiForMacrosViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new TestMidiForMacrosViewModel(MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
    //Task.Run(async () => await ViewModel.Open()).Wait();
  }

  private Settings Settings { get; set; } = null!;
  private TestMidiForMacrosViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task DisallowOverlappingContinuousCcNoRange() {
    await ViewModel.Open();
    // Check that data is as expected.
    var rangesInSettings =
      ViewModel.Settings.MidiForMacros.ContinuousCcNoRanges;
    Assert.That(rangesInSettings[0].Start, Is.EqualTo(31));
    Assert.That(rangesInSettings[0].End, Is.EqualTo(34));
    // Simulate user opting to return to the page to fix errors.
    MockDialogService.SimulatedYesNoAnswer = false;
    await DisallowOverlappingCcNoRange(
      32, 35, ViewModel.ContinuousCcNoRanges);
    Assert.That(MockDialogService.AskYesNoQuestionCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastYesNoAnswer, Is.False);
    await ViewModel.Open();
    // Check that the invalid data in the row previously added (now the last for before
    // the addition item) is still shown.
    Assert.That(ViewModel.ContinuousCcNoRanges[^2].Start, Is.EqualTo(32));
    Assert.That(ViewModel.ContinuousCcNoRanges[^2].End, Is.EqualTo(35));
  }

  [Test]
  public async Task DisallowOverlappingToggleCcNoRange() {
    await ViewModel.Open();
    // Check that data is as expected.
    var rangesInSettings =
      ViewModel.Settings.MidiForMacros.ToggleCcNoRanges;
    Assert.That(rangesInSettings[0].Start, Is.EqualTo(112));
    Assert.That(rangesInSettings[0].End, Is.EqualTo(112));
    await DisallowOverlappingCcNoRange(
      112, 113, ViewModel.ToggleCcNoRanges);
  }

  [Test]
  public async Task ProgramsNotSpecified() {
    Settings = ReadMockSettings("DefaultSettingsWithMidi.xml");
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Is.EqualTo(
      "Sound banks cannot be updated: the programs folder has not been specified."));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }

  [Test]
  public async Task UpdateAppendCcNoToMacroDisplayNames() {
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel.ConfigureMockFileSystemService(Settings);
    await ViewModel.Open();
    Assert.That(ViewModel.AppendCcNoToMacroDisplayNames, Is.True);
    ViewModel.AppendCcNoToMacroDisplayNames = false;
    Assert.That(await ViewModel.QueryClose(), Is.True);
    Assert.That(ViewModel.Settings.MidiForMacros.AppendCcNoToMacroDisplayNames,
      Is.False);
  }

  [Test]
  public async Task UpdateContinuousCcNoRanges() {
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel.ConfigureMockFileSystemService(Settings);
    await ViewModel.Open();
    await UpdateCcNoRanges(
      ViewModel.ContinuousCcNoRanges,
      ViewModel.Settings.MidiForMacros.ContinuousCcNoRanges);
  }

  [Test]
  public async Task UpdateModWheelReplacementCcNo() {
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel.ConfigureMockFileSystemService(Settings);
    await ViewModel.Open();
    const int newCcNo = 18;
    ViewModel.ModWheelReplacementCcNo = newCcNo;
    Assert.That(await ViewModel.QueryClose(), Is.True);
    Assert.That(ViewModel.Settings.MidiForMacros.ModWheelReplacementCcNo,
      Is.EqualTo(newCcNo));
  }

  [Test]
  public async Task UpdateToggleCcNoRanges() {
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel.ConfigureMockFileSystemService(Settings);
    await ViewModel.Open();
    await UpdateCcNoRanges(
      ViewModel.ToggleCcNoRanges,
      ViewModel.Settings.MidiForMacros.ToggleCcNoRanges);
  }

  [Test]
  public async Task ValidateModWheelReplacementCcNo() {
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel.ConfigureMockFileSystemService(Settings);
    await ViewModel.Open();
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
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel.ConfigureMockFileSystemService(Settings);
    await ViewModel.Open();
    // Add a new range.
    var newRange = TestHelper.CreateCcNoRangeAdditionItem(newRangeStart, newRangeEnd);
    ranges.Add(newRange);
    // Update settings.
    Assert.That(!await ViewModel.QueryClose(true));
  }

  private async Task UpdateCcNoRanges(
    CcNoRangeCollection ranges, List<Settings.IntegerRange> rangesInSettings) {
    // Add a new range.
    var newRange = TestHelper.CreateCcNoRangeAdditionItem(21, 24);
    ranges.Add(newRange);
    // Update settings.
    Assert.That(await ViewModel.QueryClose());
    Assert.That(rangesInSettings[^1].Start == newRange.Start
                && rangesInSettings[^1].End == newRange.End);
  }
}