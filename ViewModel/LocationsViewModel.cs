using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public partial class LocationsViewModel(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogService, dispatcherService) {
  // 'partial' allows CommunityToolkit.Mvvm code generation.
  private string _defaultTemplatePath = string.Empty;
  private string _originalProgramsFolderPath = string.Empty;
  private string _programsFolderPath = string.Empty;
  private string _templateProgramsFolderPath = string.Empty;
  
  [Required]
  [CustomValidation(typeof(LocationsViewModel),
    nameof(ValidateDefaultTemplatePath))]
  public string DefaultTemplatePath {
    get => _defaultTemplatePath;
    set => SetProperty(ref _defaultTemplatePath, value, true);
  }
  
  [Required]
  [CustomValidation(typeof(LocationsViewModel),
    nameof(ValidateOriginalProgramsFolderPath))]
  public string OriginalProgramsFolderPath {
    get => _originalProgramsFolderPath;
    set => SetProperty(ref _originalProgramsFolderPath, value, true);
  }
  
  [Required]
  [CustomValidation(typeof(LocationsViewModel),
    nameof(ValidateProgramsFolderPath))]
  public string ProgramsFolderPath {
    get => _programsFolderPath;
    set => SetProperty(ref _programsFolderPath, value, true);
  }
  
  [Required]
  [CustomValidation(typeof(LocationsViewModel),
    nameof(ValidateTemplateProgramsFolderPath))]
  public string TemplateProgramsFolderPath {
    get => _templateProgramsFolderPath;
    set => SetProperty(ref _templateProgramsFolderPath, value, true);
  }

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

  public static ValidationResult ValidateDefaultTemplatePath(
    string filePath, ValidationContext context) {
    return ValidateFilePath(nameof(DefaultTemplatePath), filePath, context);
  }

  public static ValidationResult ValidateOriginalProgramsFolderPath(
    string folderPath, ValidationContext context) {
    return ValidateFolderPath(nameof(OriginalProgramsFolderPath), folderPath, context);
  }

  public static ValidationResult ValidateProgramsFolderPath(
    string folderPath, ValidationContext context) {
    return ValidateFolderPath(nameof(ProgramsFolderPath), folderPath, context);
  }

  public static ValidationResult ValidateTemplateProgramsFolderPath(
    string folderPath, ValidationContext context) {
    return ValidateFolderPath(nameof(TemplateProgramsFolderPath), folderPath, context);
  }
}