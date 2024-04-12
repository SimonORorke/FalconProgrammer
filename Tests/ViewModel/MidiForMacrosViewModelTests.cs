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
    ViewModel.Open();
  }

  private MidiForMacrosViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task UpdateModWheelReplacementCcNo() {
    const int newCcNo = 18;
    ViewModel.ModWheelReplacementCcNo = newCcNo;
    Assert.That(await ViewModel.QueryCloseAsync(), Is.True);
    Assert.That(ViewModel.Settings.MidiForMacros.ModWheelReplacementCcNo,
      Is.EqualTo(newCcNo));
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
}