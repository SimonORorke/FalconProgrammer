using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class LocationsViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new LocationsViewModel(MockDialogWrapper, MockDispatcherService) {
      Navigator = MockView,
      ModelServices = TestModelServices
    };
    ViewModel.OnAppearing();
  }

  private LocationsViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task CancelBrowseForSettingsFolder() {
    MockDialogWrapper.Cancel = true;
    MockDialogWrapper.ExpectedPath = @"C:\FalconProgrammer\Settings";
    MockFileSystemService.ExpectedFileExists = false;
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.SettingsFolderPath,
      Is.Not.EqualTo(MockDialogWrapper.ExpectedPath));
  }

  [Test]
  public async Task CancelBrowseForDefaultTemplate() {
    MockDialogWrapper.Cancel = true;
    MockDialogWrapper.ExpectedPath =
      @"C:\FalconProgrammer\Program Templates\My Sound.uvip";
    var command = (AsyncRelayCommand)ViewModel.BrowseForDefaultTemplateCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.DefaultTemplatePath,
      Is.Not.EqualTo(MockDialogWrapper.ExpectedPath));
  }

  [Test]
  public async Task Main() {
    MockDialogWrapper.ExpectedPath = @"C:\FalconProgrammer\Settings";
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.SettingsFolderPath,
      Is.EqualTo(MockDialogWrapper.ExpectedPath));
    MockDialogWrapper.ExpectedPath = @"C:\FalconProgrammer\Programs";
    MockFileSystemService.ExpectedFileExists = false;
    command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.ProgramsFolderPath,
      Is.EqualTo(MockDialogWrapper.ExpectedPath));
    MockDialogWrapper.ExpectedPath = @"C:\FalconProgrammer\Original Programs";
    command = (AsyncRelayCommand)ViewModel.BrowseForOriginalProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.OriginalProgramsFolderPath,
      Is.EqualTo(MockDialogWrapper.ExpectedPath));
    MockDialogWrapper.ExpectedPath = @"C:\FalconProgrammer\Template Programs";
    command = (AsyncRelayCommand)ViewModel.BrowseForTemplateProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.TemplateProgramsFolderPath,
      Is.EqualTo(MockDialogWrapper.ExpectedPath));
    MockDialogWrapper.ExpectedPath =
      @"C:\FalconProgrammer\Program Templates\My Sound.uvip";
    command = (AsyncRelayCommand)ViewModel.BrowseForDefaultTemplateCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.DefaultTemplatePath,
      Is.EqualTo(MockDialogWrapper.ExpectedPath));
    ViewModel.OnDisappearing();
    Assert.That(MockSerialiser.LastOutputPath,
      Is.EqualTo(@"C:\FalconProgrammer\Settings\Settings.xml"));
    Assert.That(MockSerialiser.LastType, Is.EqualTo(typeof(Settings)));
    var settings = (Settings)MockSerialiser.LastObjectSerialised;
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
    ViewModel.OnAppearing();
    ViewModel.DefaultTemplatePath = @"C:\Test\Dummy.uvip";
    ViewModel.OnDisappearing();
    settings = (Settings)MockSerialiser.LastObjectSerialised;
    Assert.That(settings.DefaultTemplate.Path,
      Is.EqualTo(ViewModel.DefaultTemplatePath));
  }

  [Test]
  public async Task SettingsFolderDoesNotExist() {
    ViewModel.SettingsFolderPath = @"C:\FalconProgrammer\Settings";
    MockFileSystemService.ExpectedFileExists = false;
    MockFileSystemService.ExpectedFolderExists = false;
    MockDialogWrapper.ExpectedPath = @"C:\FalconProgrammer\Programs";
    var command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(MockDialogWrapper.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogWrapper.LastErrorMessage, Is.EqualTo(
      "Settings cannot be saved: cannot find settings folder " +
      $"'{ViewModel.SettingsFolderPath}'."));
  }

  [Test]
  public async Task SettingsFolderNotSpecified() {
    MockFileSystemService.ExpectedFileExists = false;
    MockDialogWrapper.ExpectedPath = @"C:\FalconProgrammer\Programs";
    ViewModel.SettingsFolderPath = string.Empty;
    var command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(MockDialogWrapper.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogWrapper.LastErrorMessage, Is.EqualTo(
      "Settings cannot be saved: a settings folder has not been specified."));
  }
}