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
  public async Task Settings() {
    MockFolderPicker.ExpectedPath = @"C:\Libraries";
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    MockFolderPicker.ExpectedPath = @"C:\Markup";
    MockFolderPicker.Cancel = true;
    Assert.That(ViewModel.SettingsFolderPath, 
      Is.Not.EqualTo(MockFolderPicker.ExpectedPath));
  }
}