using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class SettingsWriterViewModelBase : ViewModelBase {
  private ISettingsFolderLocation? _settingsFolderLocation;
  private string _settingsFolderPath = string.Empty;

  protected SettingsWriterViewModelBase(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  protected bool HaveSettingsBeenUpdated { get; set; }
  protected bool IsPropertyChangedNotificationEnabled { get; set; } = true;

  private ISettingsFolderLocation SettingsFolderLocation =>
    _settingsFolderLocation ??= SettingsFolderLocationReader.Read();

  /// <summary>
  ///   Gets or set the path of the settings folder, where a settings will be stored in a
  ///   settings file with the invariant name 'Settings.xml'
  /// </summary>
  /// <remarks>
  ///   Specification of the settings file name is not supported because the application
  ///   saves settings automatically, without prompting the user for the file path.
  ///   The path of a file that does not yet exist cannot be specified in advance,
  ///   whereas the path of its containing folder can.
  /// </remarks>
  [Required]
  [CustomValidation(typeof(SettingsWriterViewModelBase),
    nameof(ValidateSettingsFolderPath))]
  public string SettingsFolderPath {
    get => _settingsFolderPath;
    set => SetProperty(ref _settingsFolderPath, value, true);
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (IsVisible
        // We don't want to indicate that settings need to be saved when a new settings
        // file has just been read.
        && e.PropertyName != nameof(SettingsFolderPath)
        && IsPropertyChangedNotificationEnabled) {
      HaveSettingsBeenUpdated = true;
    }
  }

  internal override async Task Open() {
    await base.Open();
    SettingsFolderPath = SettingsFolderLocation.Path;
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    if (HaveSettingsBeenUpdated) {
      if (!TrySaveSettings(out string errorMessage)) {
        var errorReporter = new ErrorReporter(DialogService);
        bool canClosePage = await errorReporter.CanClosePageOnError(
          errorMessage, TabTitle, isClosingWindow, false);
        if (!canClosePage) {
          GoToLocationsPage();
        }
        return canClosePage;
      }
    }
    // I'm not sure whether insisting that all errors on the page are fixed is a good
    // idea. A specific check for prerequisites is instead made when attempting to open
    // the GUI Script Processor page.
    // If implemented, this needs to allow for window closing, as with the
    // TrySaveSettings error message above.
    // if (HasErrors) {
    //   await DialogService.ShowErrorMessageBox(
    //     $"You must fix the error(s) on the {TabTitle} page before continuing.");
    //   return false;
    // }
    return await base.QueryClose(isClosingWindow);
  }

  private bool TrySaveSettings(out string errorMessage) {
    if (string.IsNullOrWhiteSpace(SettingsFolderPath)) {
      errorMessage =
        "Settings cannot be saved: a settings folder has not been specified.";
      return false;
    }
    if (!FileSystemService.Folder.Exists(SettingsFolderPath)) {
      errorMessage =
        "Settings cannot be saved: cannot find settings folder "
        + $"'{SettingsFolderPath}'.";
      return false;
    }
    errorMessage = string.Empty;
    // Save settings
    if (SettingsFolderPath == SettingsFolderLocation.Path &&
        // Empty Settings.SettingsPath is the consequence of an XML error in the
        // settings folder location file.
        Settings.SettingsPath != string.Empty) {
      Settings.Write();
    } else {
      UpdateSettingsFolderLocation();
      Settings.Write(SettingsFolderPath);
    }
    HaveSettingsBeenUpdated = false;
    return true;
  }

  protected void UpdateSettingsFolderLocation() {
    SettingsFolderLocation.Path = SettingsFolderPath;
    SettingsFolderLocation.Write();
  }

  protected static ValidationResult ValidateFilePath(
    string propertyName,
    string filePath, ValidationContext context) {
    var instance = (SettingsWriterViewModelBase)context.ObjectInstance;
    bool isValid = instance.FileSystemService.File.Exists(filePath);
    return isValid
      ? ValidationResult.Success!
      : new ValidationResult("Cannot find file.", [propertyName]);
  }

  protected static ValidationResult ValidateFolderPath(
    string propertyName,
    string folderPath, ValidationContext context) {
    var instance = (SettingsWriterViewModelBase)context.ObjectInstance;
    bool isValid = instance.FileSystemService.Folder.Exists(folderPath);
    return isValid
      ? ValidationResult.Success!
      : new ValidationResult("Cannot find folder.", [propertyName]);
  }

  public static ValidationResult ValidateSettingsFolderPath(
    string folderPath, ValidationContext context) {
    return ValidateFolderPath(nameof(SettingsFolderPath), folderPath, context);
  }
}