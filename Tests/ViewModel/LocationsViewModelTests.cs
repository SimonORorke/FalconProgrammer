using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

/// <summary>
///   An unrealistic drive letter, different from the one in embedded test settings
///   files, has been chosen in these tests, as they are all expected to access a mock
///   file system. A test will throw an exception if it attempts to write to the real
///   file system, provided it is run on a computer that does not have that drive. -->
/// </summary>
[TestFixture]
public class LocationsViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new LocationsViewModel(MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
    // Do not ViewModel.Open this stage: the NoSettingsFolderLocation test
    // requires prior configuration.
  }

  private LocationsViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task CancelBrowseForSettingsFolder() {
    await ViewModel.Open();
    MockDialogService.Cancel = true;
    MockDialogService.ExpectedPath = @"K:\NewLeaf\Settings";
    MockFileSystemService.File.ExpectedExists = false;
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.SettingsFolderPath,
      Is.Not.EqualTo(MockDialogService.ExpectedPath));
  }

  [Test]
  public async Task CancelBrowseForDefaultTemplate() {
    await ViewModel.Open();
    MockDialogService.Cancel = true;
    MockDialogService.ExpectedPath =
      @"K:\NewLeaf\Program Templates\My Sound.uvip";
    var command = (AsyncRelayCommand)ViewModel.BrowseForDefaultTemplateCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.DefaultTemplatePath,
      Is.Not.EqualTo(MockDialogService.ExpectedPath));
  }

  [Test]
  public async Task LoadSettingsFromAnotherSettingsFile() {
    await ViewModel.Open();
    MockDialogService.ExpectedPath = @"K:\NewLeaf\Settings";
    MockDialogService.ExpectedYesNoAnswer = true;
    string newSettingsPath = Path.Combine(MockDialogService.ExpectedPath, "Settings.xml");
    Assert.That(ViewModel.Settings.SettingsPath, Is.Not.EqualTo(newSettingsPath));
    MockSettingsFolderLocationReader.EmbeddedFileName =
      "SettingsFolderLocationK.xml";
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(MockDialogService.AskYesNoQuestionCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastYesNoAnswer, Is.True);
    Assert.That(ViewModel.Settings.SettingsPath, Is.EqualTo(newSettingsPath));
  }

  [Test]
  public async Task Main() {
    await ViewModel.Open();
    MockDialogService.ExpectedPath = @"K:\NewLeaf\Settings";
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.SettingsFolderPath,
      Is.EqualTo(MockDialogService.ExpectedPath));
    MockDialogService.ExpectedPath = @"K:\NewLeaf\Programs";
    MockFileSystemService.File.ExpectedExists = false;
    command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.ProgramsFolderPath,
      Is.EqualTo(MockDialogService.ExpectedPath));
    MockDialogService.ExpectedPath = @"K:\NewLeaf\Original Programs";
    command = (AsyncRelayCommand)ViewModel.BrowseForOriginalProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.OriginalProgramsFolderPath,
      Is.EqualTo(MockDialogService.ExpectedPath));
    MockDialogService.ExpectedPath = @"K:\NewLeaf\Template Programs";
    command = (AsyncRelayCommand)ViewModel.BrowseForTemplateProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.TemplateProgramsFolderPath,
      Is.EqualTo(MockDialogService.ExpectedPath));
    MockDialogService.ExpectedPath =
      @"K:\NewLeaf\Program Templates\My Sound.uvip";
    command = (AsyncRelayCommand)ViewModel.BrowseForDefaultTemplateCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.DefaultTemplatePath,
      Is.EqualTo(MockDialogService.ExpectedPath));
    await ViewModel.QueryClose();
    var mockSerialiser = (MockSerialiser)ViewModel.Settings.Serialiser;
    Assert.That(mockSerialiser.LastOutputPath,
      Is.EqualTo(@"K:\NewLeaf\Settings\Settings.xml"));
    Assert.That(mockSerialiser.LastType, Is.EqualTo(typeof(Settings)));
    var settings = (Settings)mockSerialiser.LastObjectSerialised;
    Assert.That(settings.SettingsPath,
      Is.EqualTo(Path.Combine(ViewModel.SettingsFolderPath, "Settings.xml")));
    Assert.That(settings.ProgramsFolder.Path, Is.EqualTo(ViewModel.ProgramsFolderPath));
    Assert.That(settings.OriginalProgramsFolder.Path,
      Is.EqualTo(ViewModel.OriginalProgramsFolderPath));
    Assert.That(settings.TemplateProgramsFolder.Path,
      Is.EqualTo(ViewModel.TemplateProgramsFolderPath));
    Assert.That(settings.DefaultTemplate.Path,
      Is.EqualTo(ViewModel.DefaultTemplatePath));
    // Test that the settings folder path when writing settings is now already as
    // specified in the settings folder location file. 
    await ViewModel.Open();
    ViewModel.DefaultTemplatePath = @"K:\Test\Dummy.uvip";
    await ViewModel.QueryClose();
    settings = (Settings)mockSerialiser.LastObjectSerialised;
    Assert.That(settings.DefaultTemplate.Path,
      Is.EqualTo(ViewModel.DefaultTemplatePath));
  }

  [Test]
  public async Task NoSettingsFolderLocation() {
    MockSettingsFolderLocationReader.ExpectedFileExists = false;
    await ViewModel.Open();
    Assert.That(ViewModel.SettingsFolderPath, Is.Empty);
  }

  [Test]
  public async Task SettingsFolderDoesNotExist() {
    await ViewModel.Open();
    ViewModel.SettingsFolderPath = @"K:\NewLeaf\Settings";
    MockFileSystemService.File.ExpectedExists = false;
    MockFileSystemService.Folder.ExpectedExists = false;
    MockDialogService.ExpectedPath = @"K:\NewLeaf\Programs";
    // Make a property change to require saving settings.
    ViewModel.ProgramsFolderPath += "X";
    Assert.That(await ViewModel.QueryClose(), Is.False);
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Is.EqualTo(
      "Settings cannot be saved: cannot find settings folder " +
      $"'{ViewModel.SettingsFolderPath}'."));
  }

  [Test]
  public async Task SettingsFolderNotSpecified() {
    await ViewModel.Open();
    MockFileSystemService.File.ExpectedExists = false;
    MockDialogService.ExpectedPath = @"K:\NewLeaf\Programs";
    ViewModel.SettingsFolderPath = string.Empty;
    Assert.That(await ViewModel.QueryClose(), Is.False);
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Is.EqualTo(
      "Settings cannot be saved: a settings folder has not been specified."));
  }
}