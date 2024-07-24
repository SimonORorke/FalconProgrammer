using FalconProgrammer.Model;
using FalconProgrammer.Model.Mpe;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MpeViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    MockSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    ViewModel = new MpeViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  private MpeViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task Main() {
    await ViewModel.Open();
    Assert.That(Global.GetEnumValue<YTarget>(ViewModel.YTargets[0]), 
      Is.EqualTo(YTarget.None));
    Assert.That(Global.GetEnumValue<ZTarget>(ViewModel.ZTargets[0]), 
      Is.EqualTo(ZTarget.None));
    Assert.That(Global.GetEnumValue<XTarget>(ViewModel.XTargets[0]), 
      Is.EqualTo(XTarget.None));
    Assert.That(Global.GetEnumValue<YTarget>(ViewModel.YTarget), 
      Is.EqualTo(YTarget.ContinuousMacro1Bipolar));
    Assert.That(Global.GetEnumValue<ZTarget>(ViewModel.ZTarget), 
      Is.EqualTo(ZTarget.ContinuousMacro2Unipolar));
    Assert.That(Global.GetEnumValue<XTarget>(ViewModel.XTarget), 
      Is.EqualTo(XTarget.ContinuousMacro3Bipolar));
    const YTarget newYTarget = YTarget.PolyphonicAftertouch;
    const ZTarget newZTarget = ZTarget.Gain;
    const XTarget newXTarget = XTarget.Pitch;
    ViewModel.YTarget = newYTarget.ToString();
    ViewModel.ZTarget = newZTarget.ToString();
    ViewModel.XTarget = newXTarget.ToString();
    bool hasClosed = await ViewModel.QueryClose();
    Assert.That(hasClosed, Is.True);
    Assert.That(ViewModel.Settings.Mpe.YTargetValue, Is.EqualTo(newYTarget));
    Assert.That(ViewModel.Settings.Mpe.ZTargetValue, Is.EqualTo(newZTarget));
    Assert.That(ViewModel.Settings.Mpe.XTargetValue, Is.EqualTo(newXTarget));
  }
}