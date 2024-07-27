using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class SoundBankSpecificViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    MockSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    ViewModel = new SoundBankSpecificViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  private SoundBankSpecificViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task OrganicPads() {
    await ViewModel.Open();
    ViewModel.OrganicPadsAttackSeconds = -1;
    Assert.That(ViewModel.HasErrors, Is.True);
    MockDialogService.SimulatedYesNoAnswer = false;
    bool hasClosed = await ViewModel.QueryClose();
    Assert.That(hasClosed, Is.False);
    ViewModel.OrganicPadsAttackSeconds = null;
    Assert.That(ViewModel.HasErrors, Is.False);
    hasClosed = await ViewModel.QueryClose();
    Assert.That(hasClosed, Is.True);
    Assert.That(ViewModel.Settings.SoundBankSpecific.OrganicPads.AttackSeconds,
      Is.EqualTo(-1));
    await ViewModel.Open();
    ViewModel.OrganicPadsReleaseSeconds = 99;
    Assert.That(ViewModel.HasErrors, Is.True);
    MockDialogService.SimulatedYesNoAnswer = true;
    hasClosed = await ViewModel.QueryClose();
    Assert.That(hasClosed, Is.False);
    ViewModel.OrganicPadsReleaseSeconds = 3;
    hasClosed = await ViewModel.QueryClose();
    Assert.That(hasClosed, Is.True);
    Assert.That(ViewModel.Settings.SoundBankSpecific.OrganicPads.ReleaseSeconds,
      Is.EqualTo(3));
  }
}