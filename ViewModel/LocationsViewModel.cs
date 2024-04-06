using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public partial class LocationsViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogWrapper, dispatcherService) {
  [ObservableProperty] private string _defaultTemplatePath = string.Empty;
  [ObservableProperty] private string _originalProgramsFolderPath = string.Empty;
  [ObservableProperty] private string _programsFolderPath = string.Empty;
  [ObservableProperty] private string _templateProgramsFolderPath = string.Empty;

  public override string PageTitle => "Locations";

  [RelayCommand]
  private async Task BrowseForDefaultTemplate() {
    string? path = await DialogWrapper.BrowseForFileAsync(this,
      "Select the default template Falcon program",
      "Falcon Programs", "uvip");
    if (path != null) {
      DefaultTemplatePath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForOriginalProgramsFolder() {
    string? path = await DialogWrapper.BrowseForFolderAsync(this,
      "Select the Original Programs folder");
    if (path != null) {
      OriginalProgramsFolderPath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForProgramsFolder() {
    string? path = await DialogWrapper.BrowseForFolderAsync(this,
      "Select the Programs folder");
    if (path != null) {
      ProgramsFolderPath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForSettingsFolder() {
    string? path = await DialogWrapper.BrowseForFolderAsync(this,
      "Select the Settings folder");
    if (path != null) {
      SettingsFolderPath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForTemplateProgramsFolder() {
    string? path = await DialogWrapper.BrowseForFolderAsync(this,
      "Select the Template Programs folder");
    if (path != null) {
      TemplateProgramsFolderPath = path;
    }
  }
}