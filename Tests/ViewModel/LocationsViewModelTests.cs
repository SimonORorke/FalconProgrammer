using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class LocationsViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new LocationsViewModel {
      ServiceHelper = ServiceHelper
    };
    ViewModel.OnAppearing();
  }

  private LocationsViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task SettingsFolder() {
    MockFolderPicker.ExpectedPath = @"C:\Libraries";
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    MockFolderPicker.ExpectedPath = @"C:\Markup";
    MockFolderPicker.Cancel = true;
    Assert.That(ViewModel.SettingsFolderPath, 
      Is.Not.EqualTo(MockFolderPicker.ExpectedPath));
  }

  [Test]
  public async Task SettingsFolderDoesNotExist() {
    ViewModel.SettingsFolderPath = @"C:\Libraries";
    MockFileSystemService.ExpectedFolderExists = false;
    var command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(MockAlertService.ShowAlertCount, Is.EqualTo(1));
    Assert.That(MockAlertService.LastTitle, Is.EqualTo("Error"));
    Assert.That(MockAlertService.LastMessage, Is.EqualTo(
      @"Settings cannot be saved: cannot find settings folder 'C:\Libraries'."));
  }

  [Test]
  public async Task SettingsFolderNotSpecified() {
    var command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(MockAlertService.ShowAlertCount, Is.EqualTo(1));
    Assert.That(MockAlertService.LastTitle, Is.EqualTo("Error"));
    Assert.That(MockAlertService.LastMessage, Is.EqualTo(
      "Settings cannot be saved: a settings folder has not been specified."));
  }
}