using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class BackgroundViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = ReadMockSettings("LocationsSettings.xml");
    ViewModel = new BackgroundViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  private Settings Settings { get; set; } = null!;
  private BackgroundViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task Main() {
    TestHelper.AddSoundBankSubfolders(
      MockFileSystemService.Folder, Settings.ProgramsFolder.Path);
    await ViewModel.Open(); // Reads settings to populate the page.
    // Check that the initial settings are as expected
    int initialSettingsBackgroundCount = ViewModel.Settings.Backgrounds.Count;
    Assert.That(initialSettingsBackgroundCount, Is.EqualTo(2));
    Assert.That(ViewModel.Settings.Backgrounds[0].Path,
      Is.EqualTo(@"J:\FalconProgrammer\Background Images\Yellowish Mid-Green.png"));
    const string newPath =
      @"J:\FalconProgrammer\Background Images\Dark Forest.png";
    ViewModel.Backgrounds[0].Path = newPath;
    await ViewModel.QueryClose(); // Updates and saves settings
    Assert.That(ViewModel.Settings.Backgrounds[0].Path, Is.EqualTo(newPath));
  }

  [Test]
  public async Task ProgramsFolderNotFound() {
    MockFileSystemService.Folder.SimulatedExists = false;
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Background images cannot be updated: cannot find programs folder "));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }
}