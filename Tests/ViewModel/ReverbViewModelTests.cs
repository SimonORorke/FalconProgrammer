using FalconProgrammer.Model.Options;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class ReverbViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = ReadMockSettings("BatchSettings.xml");
    ViewModel = new ReverbViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
  }

  private Settings Settings { get; set; } = null!;
  private ReverbViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task InvalidProgramItem() {
    TestHelper.AddSoundBankSubfolders(
      MockFileSystemService.Folder, Settings.ProgramsFolder.Path);
    await ViewModel.Open(); // Reads settings to populate the page.
    var invalidProgramItem =
      new ProgramItem(ViewModel.Settings, ViewModel.FileSystemService,
        false, true) {
        SoundBank = "Pulsar", Category = "Plucks", Program = string.Empty
      };
    ViewModel.DoNotZeroReverb[^1] = invalidProgramItem; // Addition item
    MockDialogService.SimulatedYesNoAnswer = false;
    bool canClose = await ViewModel.QueryClose();
    Assert.That(canClose, Is.EqualTo(false));
  }

  [Test]
  public async Task Main() {
    TestHelper.AddSoundBankSubfolders(
      MockFileSystemService.Folder, Settings.ProgramsFolder.Path);
    await ViewModel.Open(); // Reads settings to populate the page.
    // Check that the initial settings are as expected
    int initialSettingsReverbCount = ViewModel.Settings.DoNotZeroReverb.Count;
    Assert.That(initialSettingsReverbCount, Is.EqualTo(20));
    Assert.That(GetPathShort(ViewModel.Settings.DoNotZeroReverb[0]),
      Is.EqualTo(@"Falcon Factory\Bass-Sub\Coastal Halftones 1.4"));
    var newProgramItem =
      new ProgramItem(ViewModel.Settings, ViewModel.FileSystemService,
        false, true) {
        SoundBank = "Pulsar", Category = "Plucks", Program = "C"
      };
    ViewModel.DoNotZeroReverb[0] = newProgramItem;
    bool canClose = await ViewModel.QueryClose(); // Updates and saves settings
    Assert.That(canClose, Is.EqualTo(true));
    Assert.That(GetPathShort(ViewModel.Settings.DoNotZeroReverb[0]),
      Is.EqualTo(Path.Combine(newProgramItem.SoundBank,
        newProgramItem.Category, newProgramItem.Program)));
    return;

    string GetPathShort(ProgramPath programPath) {
      return Path.Combine(programPath.SoundBank,
        programPath.Category, programPath.Program);
    }
  }

  [Test]
  public async Task ProgramsFolderNotFound() {
    MockFileSystemService.Folder.SimulatedExists = false;
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Reverb cannot be updated: cannot find programs folder "));
    Assert.That(MockMessageRecipient.GoToLocationsPageCount, Is.EqualTo(1));
  }
}