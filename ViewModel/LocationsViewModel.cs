using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace FalconProgrammer.ViewModel;

public partial class LocationsViewModel : SettingsWriterViewModelBase {
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
    string? path = await FileChooser.ChooseAsync(
      "Select the default template Falcon program", "*.uvip");
    if (path != null) {
      DefaultTemplatePath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForOriginalProgramsFolder() {
    string? path = await FolderChooser.ChooseAsync();
    if (path != null) {
      OriginalProgramsFolderPath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForProgramsFolder() {
    string? path = await FolderChooser.ChooseAsync();
    if (path != null) {
      ProgramsFolderPath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForSettingsFolder() {
    string? path = await FolderChooser.ChooseAsync();
    if (path != null) {
      SettingsFolderPath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForTemplateProgramsFolder() {
    string? path = await FolderChooser.ChooseAsync();
    if (path != null) {
      TemplateProgramsFolderPath = path;
    }
  }
}