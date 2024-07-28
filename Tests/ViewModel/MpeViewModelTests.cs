using FalconProgrammer.Model;
using FalconProgrammer.Model.Mpe;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MpeViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new MpeViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  private MpeViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task Defaults() {
    MockSettingsReaderEmbedded.EmbeddedFileName = "DefaultSettingsWithMidi.xml";
    await ViewModel.Open();
    Assert.That(Global.GetEnumValue<YTarget>(ViewModel.YTarget),
      Is.EqualTo(YTarget.ContinuousMacro1Bipolar));
    Assert.That(Global.GetEnumValue<ZTarget>(ViewModel.ZTarget),
      Is.EqualTo(ZTarget.ContinuousMacro2Unipolar));
    Assert.That(Global.GetEnumValue<XTarget>(ViewModel.XTarget),
      Is.EqualTo(XTarget.Pitch));
    Assert.That(ViewModel.GainMapDisplayName, Is.EqualTo("20 dB"));
    Assert.That(ViewModel.InitialiseZToMacroValue, Is.False);
    Assert.That(ViewModel.PitchBendRange, Is.EqualTo(48));
    const ZTarget newZTarget = ZTarget.ContinuousMacro2Bipolar;
    const string newGainMapDisplayName = "Z Squared";
    const GainMap newGainMap = GainMap.ZSquared;
    ViewModel.ZTarget = newZTarget.ToString();
    ViewModel.GainMapDisplayName = newGainMapDisplayName;
    bool hasClosed = await ViewModel.QueryClose();
    Assert.That(hasClosed, Is.True);
    Assert.That(ViewModel.Settings.Mpe.ZTargetValue, Is.EqualTo(newZTarget));
    Assert.That(ViewModel.Settings.Mpe.GainMap, Is.EqualTo(newGainMap.ToString()));
  }

  [Test]
  public async Task Main() {
    MockSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    await ViewModel.Open();
    Assert.That(Global.GetEnumValue<YTarget>(ViewModel.YTargets[0]),
      Is.EqualTo(YTarget.None));
    Assert.That(Global.GetEnumValue<ZTarget>(ViewModel.ZTargets[0]),
      Is.EqualTo(ZTarget.None));
    Assert.That(Global.GetEnumValue<XTarget>(ViewModel.XTargets[0]),
      Is.EqualTo(XTarget.None));
    Assert.That(ViewModel.GainMapDisplayNames[2], Is.EqualTo("Linear"));
    Assert.That(Global.GetEnumValue<YTarget>(ViewModel.YTarget),
      Is.EqualTo(YTarget.ContinuousMacro1Bipolar));
    Assert.That(Global.GetEnumValue<ZTarget>(ViewModel.ZTarget),
      Is.EqualTo(ZTarget.ContinuousMacro2Unipolar));
    Assert.That(Global.GetEnumValue<XTarget>(ViewModel.XTarget),
      Is.EqualTo(XTarget.ContinuousMacro3Bipolar));
    Assert.That(ViewModel.GainMapDisplayName, Is.EqualTo("Linear"));
    Assert.That(ViewModel.InitialiseZToMacroValue, Is.True);
    Assert.That(ViewModel.PitchBendRange, Is.EqualTo(24));
    const YTarget newYTarget = YTarget.PolyphonicAftertouch;
    const ZTarget newZTarget = ZTarget.Gain;
    const XTarget newXTarget = XTarget.Pitch;
    const string newGainMapDisplayName = "20 dB";
    const GainMap newGainMap = GainMap.TwentyDb;
    const int newPitchBendRange = 12;
    ViewModel.YTarget = newYTarget.ToString();
    ViewModel.InitialiseZToMacroValue = false;
    ViewModel.ZTarget = newZTarget.ToString();
    ViewModel.XTarget = newXTarget.ToString();
    ViewModel.GainMapDisplayName = newGainMapDisplayName;
    ViewModel.PitchBendRange = newPitchBendRange;
    bool hasClosed = await ViewModel.QueryClose();
    Assert.That(hasClosed, Is.True);
    Assert.That(ViewModel.Settings.Mpe.YTargetValue, Is.EqualTo(newYTarget));
    Assert.That(ViewModel.Settings.Mpe.ZTargetValue, Is.EqualTo(newZTarget));
    Assert.That(ViewModel.Settings.Mpe.XTargetValue, Is.EqualTo(newXTarget));
    Assert.That(ViewModel.Settings.Mpe.GainMap, Is.EqualTo(newGainMap.ToString()));
    Assert.That(ViewModel.Settings.Mpe.InitialiseZToMacroValue, Is.False);
    Assert.That(ViewModel.Settings.Mpe.PitchBendRange, Is.EqualTo(newPitchBendRange));
  }
}