using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public partial class LocationsViewModel : SettingsWriterViewModelBase {
  // 'partial' allows CommunityToolkit.Mvvm code generation.
  private string _defaultTemplatePath = string.Empty;
  private string _originalProgramsFolderPath = string.Empty;
  private string _programsFolderPath = string.Empty;
  private string _templateProgramsFolderPath = string.Empty;

  public LocationsViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [Required]
  [CustomValidation(typeof(LocationsViewModel),
    nameof(ValidateDefaultTemplatePath))]
  public string DefaultTemplatePath {
    get => _defaultTemplatePath;
    set => SetProperty(ref _defaultTemplatePath, value, true);
  }

  private string FoundSettingsPath { get; set; } = string.Empty;

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
      string newFoundSettingsPath = FindSettingsFile();
      if (newFoundSettingsPath != FoundSettingsPath) {
        FoundSettingsPath = newFoundSettingsPath;
        if (FoundSettingsPath != string.Empty) {
          bool load = await DialogService.AskYesNoQuestionAsync(
            $"Settings file '{FoundSettingsPath}' already exists. " +
            "Do you want to load the settings from that file?");
          if (load) {
            await LoadSettingsFromNewFile();
          }
        }
      }
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

  /// <summary>
  ///   If a settings folder has been specified and is a folder containing the settings
  ///   file Settings.xml, returns the path of the file. Otherwise returns an empty
  ///   string.
  /// </summary>
  private string FindSettingsFile() {
    if (string.IsNullOrWhiteSpace(SettingsFolderPath)) {
      return string.Empty;
    }
    string settingsPath = Path.Combine(SettingsFolderPath, "Settings.xml");
    return FileSystemService.File.Exists(settingsPath)
      ? settingsPath
      : string.Empty;
  }

  private async Task LoadSettingsFromNewFile() {
    UpdateSettingsFolderLocation();
    await ReadSettingsAsync();
    ShowPathSettings();
  }

  protected override async Task OnSettingsXmlErrorAsync(string errorMessage) {
    await base.OnSettingsXmlErrorAsync(errorMessage);
    await DialogService.ShowErrorMessageBoxAsync(errorMessage);
  }

  internal override async Task Open() {
    await base.Open();
    ShowPathSettings();
    FoundSettingsPath = FindSettingsFile();
  }

  private void ShowPathSettings() {
    // We don't want to indicate that settings need to be saved when a new settings
    // file has just been read.
    IsPropertyChangedNotificationEnabled = false;
    DefaultTemplatePath = Settings.DefaultTemplate.Path;
    OriginalProgramsFolderPath = Settings.OriginalProgramsFolder.Path;
    ProgramsFolderPath = Settings.ProgramsFolder.Path;
    TemplateProgramsFolderPath = Settings.TemplateProgramsFolder.Path;
    IsPropertyChangedNotificationEnabled = true;
  }

  internal override async Task<bool> QueryCloseAsync(bool isClosingWindow = false) {
    Settings.DefaultTemplate.Path = DefaultTemplatePath;
    Settings.OriginalProgramsFolder.Path = OriginalProgramsFolderPath;
    Settings.ProgramsFolder.Path = ProgramsFolderPath;
    Settings.TemplateProgramsFolder.Path = TemplateProgramsFolderPath;
    return await base.QueryCloseAsync(isClosingWindow); // Saves settings if changed.
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