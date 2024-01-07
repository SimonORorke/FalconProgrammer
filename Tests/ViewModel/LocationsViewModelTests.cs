using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class LocationsViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new LocationsViewModel {
      Serializer = MockSerializer,
      ServiceHelper = ServiceHelper
    };
    ViewModel.OnAppearing();
  }

  private LocationsViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task CancelBrowseForSettingsFolder() {
    MockFolderPicker.Cancel = true;
    MockFolderPicker.ExpectedPath = @"C:\FalconProgrammer\Settings";
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.SettingsFolderPath, 
      Is.Not.EqualTo(MockFolderPicker.ExpectedPath));
  }

  [Test]
  public async Task CancelBrowseForDefaultTemplate() {
    MockFilePicker.Cancel = true;
    MockFilePicker.ExpectedPath = @"C:\FalconProgrammer\Program Templates\My Sound.uvip";
    var command = (AsyncRelayCommand)ViewModel.BrowseForDefaultTemplateCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.DefaultTemplatePath, 
      Is.Not.EqualTo(MockFilePicker.ExpectedPath));
  }

  [Test]
  public async Task Main() {
    MockFolderPicker.ExpectedPath = @"C:\FalconProgrammer\Settings";
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.SettingsFolderPath,
      Is.EqualTo(MockFolderPicker.ExpectedPath));
    MockFolderPicker.ExpectedPath = @"C:\FalconProgrammer\Programs";
    command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.ProgramsFolderPath,
      Is.EqualTo(MockFolderPicker.ExpectedPath));
    MockFolderPicker.ExpectedPath = @"C:\FalconProgrammer\Original Programs";
    command = (AsyncRelayCommand)ViewModel.BrowseForOriginalProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.OriginalProgramsFolderPath,
      Is.EqualTo(MockFolderPicker.ExpectedPath));
    MockFolderPicker.ExpectedPath = @"C:\FalconProgrammer\Template Programs";
    command = (AsyncRelayCommand)ViewModel.BrowseForTemplateProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.TemplateProgramsFolderPath,
      Is.EqualTo(MockFolderPicker.ExpectedPath));
    MockFilePicker.ExpectedPath = @"C:\FalconProgrammer\Program Templates\My Sound.uvip";
    command = (AsyncRelayCommand)ViewModel.BrowseForDefaultTemplateCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.DefaultTemplatePath, 
      Is.EqualTo(MockFilePicker.ExpectedPath));
    ViewModel.OnDisappearing();
    Assert.That(MockSerializer.SerializeCount, Is.EqualTo(1));
    Assert.That(MockSerializer.LastOutputPath, 
      Is.EqualTo(@"C:\FalconProgrammer\Settings\Settings.xml"));
    Assert.That(MockSerializer.LastType, Is.EqualTo(typeof(Settings)));
    var settings = (Settings)MockSerializer.LastObjectSerialised;
  }

  [Test]
  public async Task SettingsFolderDoesNotExist() {
    ViewModel.SettingsFolderPath = @"C:\FalconProgrammer\Settings";
    MockFileSystemService.ExpectedFolderExists = false;
    MockFolderPicker.ExpectedPath = @"C:\FalconProgrammer\Programs";
    var command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(MockAlertService.ShowAlertCount, Is.EqualTo(1));
    Assert.That(MockAlertService.LastTitle, Is.EqualTo("Error"));
    Assert.That(MockAlertService.LastMessage, Is.EqualTo(
      "Settings cannot be saved: cannot find settings folder " + 
      $"'{ViewModel.SettingsFolderPath}'."));
  }

  [Test]
  public async Task SettingsFolderNotSpecified() {
    MockFolderPicker.ExpectedPath = @"C:\FalconProgrammer\Programs";
    var command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(MockAlertService.ShowAlertCount, Is.EqualTo(1));
    Assert.That(MockAlertService.LastTitle, Is.EqualTo("Error"));
    Assert.That(MockAlertService.LastMessage, Is.EqualTo(
      "Settings cannot be saved: a settings folder has not been specified."));
  }
}