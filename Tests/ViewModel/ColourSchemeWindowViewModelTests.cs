using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class ColourSchemeWindowViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new ColourSchemeWindowViewModel(ColourSchemeId.Forest,
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  private ColourSchemeWindowViewModel ViewModel { get; set; } = null!;

  [Test]
  public void ColourSchemes() {
    Assert.That(ViewModel.ColourSchemes[0],
      Is.EqualTo(ColourSchemeId.Default.ToString()));
  }

  [Test]
  public async Task SettingFoundChange() {
    MockSettingsReaderEmbedded.EmbeddedFileName = "BatchSettings.xml";
    var newColourSchemeId = ColourSchemeId.Default;
    ViewModel.ChangeColourScheme += (_, colourSchemeId) =>
      newColourSchemeId = colourSchemeId;
    await ViewModel.Open();
    Assert.That(ViewModel.ColourScheme, Is.EqualTo(ColourSchemeId.Forest.ToString()));
    string newColourScheme = ColourSchemeId.Nighttime.ToString();
    ViewModel.ColourScheme = newColourScheme;
    Assert.That(ViewModel.Settings.ColourScheme, Is.EqualTo(newColourScheme));
    Assert.That(newColourSchemeId, Is.EqualTo(ColourSchemeId.Nighttime));
  }
}