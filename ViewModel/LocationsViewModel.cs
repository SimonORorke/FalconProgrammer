using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace FalconProgrammer.ViewModel;

public partial class LocationsViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogWrapper, dispatcherService) {
  // 'partial' allows CommunityToolkit.Mvvm code generation based on ObservableProperty
  // and RelayCommand attributes.

  public string DefaultTemplatePath {
    get => Settings.DefaultTemplate.Path;
    set {
      if (Settings.DefaultTemplate.Path != value) {
        Settings.DefaultTemplate.Path = value;
        OnPropertyChanged();
      }
    }
  }

  [PublicAPI]
  public string OriginalProgramsFolderPath {
    get => Settings.OriginalProgramsFolder.Path;
    set {
      if (Settings.OriginalProgramsFolder.Path != value) {
        Settings.OriginalProgramsFolder.Path = value;
        OnPropertyChanged();
      }
    }
  }

  public override string PageTitle => "Locations";

  [PublicAPI]
  public string ProgramsFolderPath {
    get => Settings.ProgramsFolder.Path;
    set {
      if (Settings.ProgramsFolder.Path != value) {
        Settings.ProgramsFolder.Path = value;
        OnPropertyChanged();
      }
    }
  }

  [PublicAPI]
  public string TemplateProgramsFolderPath {
    get => Settings.TemplateProgramsFolder.Path;
    set {
      if (Settings.TemplateProgramsFolder.Path != value) {
        Settings.TemplateProgramsFolder.Path = value;
        OnPropertyChanged();
      }
    }
  }

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