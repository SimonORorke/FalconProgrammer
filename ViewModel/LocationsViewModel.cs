using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public partial class LocationsViewModel : SettingsWriterViewModelBase {
  // 'partial' allows CommunityToolkit.Mvvm code generation.
  private string _originalProgramsFolderPath = string.Empty;
  private string _programsFolderPath = string.Empty;
  private string _templateProgramsFolderPath = string.Empty;

  public LocationsViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

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

  [CustomValidation(typeof(LocationsViewModel),
    nameof(ValidateTemplateProgramsFolderPath))]
  public string TemplateProgramsFolderPath {
    get => _templateProgramsFolderPath;
    set => SetProperty(ref _templateProgramsFolderPath, value, true);
  }

  public override string PageTitle => "Locations";

  /// <summary>
  ///   Generates <see cref="BrowseForOriginalProgramsFolderCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task BrowseForOriginalProgramsFolder() {
    string? path = await DialogService.BrowseForFolder(
      "Select the Original Programs folder");
    if (path != null) {
      OriginalProgramsFolderPath = path;
    }
  }

  /// <summary>
  ///   Generates <see cref="BrowseForProgramsFolderCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task BrowseForProgramsFolder() {
    string? path = await DialogService.BrowseForFolder(
      "Select the Programs folder");
    if (path != null) {
      ProgramsFolderPath = path;
    }
  }

  /// <summary>
  ///   Generates <see cref="BrowseForSettingsFolderCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task BrowseForSettingsFolder() {
    string? path = await DialogService.BrowseForFolder(
      "Select the Settings folder");
    if (path != null) {
      SettingsFolderPath = path;
      string newFoundSettingsPath = FindSettingsFile();
      if (newFoundSettingsPath != FoundSettingsPath) {
        FoundSettingsPath = newFoundSettingsPath;
        if (FoundSettingsPath != string.Empty) {
          bool load = await DialogService.AskYesNoQuestion(
            $"Settings file '{FoundSettingsPath}' already exists. " +
            "Do you want to load the settings from that file?");
          if (load) {
            await LoadSettingsFromNewFile();
          }
        }
      }
    }
  }

  /// <summary>
  ///   Generates <see cref="BrowseForTemplateProgramsFolderCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task BrowseForTemplateProgramsFolder() {
    string? path = await DialogService.BrowseForFolder(
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
    await ReadSettings();
    ShowPathSettings();
  }

  protected override async Task OnSettingsXmlError(string errorMessage) {
    await base.OnSettingsXmlError(errorMessage);
    await DialogService.ShowErrorMessageBox(errorMessage);
  }

  internal override async Task Open() {
    await base.Open();
    ShowPathSettings();
    FoundSettingsPath = FindSettingsFile();
  }

  private void ShowPathSettings() {
    // We don't want to indicate that settings need to be saved when a new settings
    // file has just been read.
    FlagSettingsUpdateOnPropertyChanged = false;
    OriginalProgramsFolderPath = Settings.OriginalProgramsFolder.Path;
    ProgramsFolderPath = Settings.ProgramsFolder.Path;
    TemplateProgramsFolderPath = Settings.TemplateProgramsFolder.Path;
    FlagSettingsUpdateOnPropertyChanged = true;
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    Settings.OriginalProgramsFolder.Path = OriginalProgramsFolderPath;
    Settings.ProgramsFolder.Path = ProgramsFolderPath;
    Settings.TemplateProgramsFolder.Path = TemplateProgramsFolderPath;
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
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
    return !string.IsNullOrEmpty(folderPath)
      ? ValidateFolderPath(nameof(TemplateProgramsFolderPath), folderPath, context)
      : ValidationResult.Success!;
  }
}