using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public partial class LocationsViewModel(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogService, dispatcherService) {
  // 'partial' allows CommunityToolkit.Mvvm code generation based on ObservableProperty
  // and RelayCommand attributes.
  
  /// <summary>
  ///   Generates the DefaultTemplatePath property.
  /// </summary>
  [ObservableProperty] private string _defaultTemplatePath = string.Empty;
  
  /// <summary>
  ///   Generates the OriginalProgramsFolderPath property.
  /// </summary>
  [ObservableProperty] private string _originalProgramsFolderPath = string.Empty;
  
  /// <summary>
  ///   Generates the ProgramsFolderPath property.
  /// </summary>
  [ObservableProperty] private string _programsFolderPath = string.Empty;
  
  /// <summary>
  ///   Generates the TemplateProgramsFolderPath property.
  /// </summary>
  [ObservableProperty] private string _templateProgramsFolderPath = string.Empty;

  public override string PageTitle => "Locations";

  [RelayCommand]
  private async Task BrowseForDefaultTemplate() {
    string? path = await DialogService.BrowseForFileAsync(
      "Select the default template Falcon program",
      "Falcon Programs", "uvip");
    if (path != null) {
      DefaultTemplatePath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForOriginalProgramsFolder() {
    string? path = await DialogService.BrowseForFolderAsync(
      "Select the Original Programs folder");
    if (path != null) {
      OriginalProgramsFolderPath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForProgramsFolder() {
    string? path = await DialogService.BrowseForFolderAsync(
      "Select the Programs folder");
    if (path != null) {
      ProgramsFolderPath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForSettingsFolder() {
    string? path = await DialogService.BrowseForFolderAsync(
      "Select the Settings folder");
    if (path != null) {
      SettingsFolderPath = path;
    }
  }

  [RelayCommand]
  private async Task BrowseForTemplateProgramsFolder() {
    string? path = await DialogService.BrowseForFolderAsync(
      "Select the Template Programs folder");
    if (path != null) {
      TemplateProgramsFolderPath = path;
    }
  }
  
  public override void Open() {
    base.Open();
    DefaultTemplatePath = Settings.DefaultTemplate.Path;
    OriginalProgramsFolderPath = Settings.OriginalProgramsFolder.Path;
    ProgramsFolderPath = Settings.ProgramsFolder.Path;
    TemplateProgramsFolderPath = Settings.TemplateProgramsFolder.Path; 
  }

  public override bool QueryClose() {
    Settings.DefaultTemplate.Path = DefaultTemplatePath;
    Settings.OriginalProgramsFolder.Path = OriginalProgramsFolderPath;
    Settings.ProgramsFolder.Path = ProgramsFolderPath;
    Settings.TemplateProgramsFolder.Path = TemplateProgramsFolderPath; 
    return base.QueryClose(); // Saves settings if changed.
  }
}