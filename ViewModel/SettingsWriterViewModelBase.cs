using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class SettingsWriterViewModelBase(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogService, dispatcherService) {
  private ISettingsFolderLocation? _settingsFolderLocation;
  private string _settingsFolderPath = string.Empty;
  private bool HaveSettingsBeenUpdated { get; set; }

  private ISettingsFolderLocation SettingsFolderLocation =>
    _settingsFolderLocation ??= SettingsFolderLocationReader.Read();

  [Required]
  [CustomValidation(typeof(SettingsWriterViewModelBase),
    nameof(ValidateSettingsFolderPath))]
  public string SettingsFolderPath {
    get => _settingsFolderPath;
    set => SetProperty(ref _settingsFolderPath, value, true);
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (IsVisible) {
      HaveSettingsBeenUpdated = true;
    }
  }

  internal override async Task Open() {
    await base.Open();
    SettingsFolderPath = SettingsFolderLocation.Path;
  }

  internal override async Task<bool> QueryCloseAsync(bool isClosingWindow = false) {
    if (HaveSettingsBeenUpdated) {
      if (!TrySaveSettings(out string errorMessage)) {
        var errorReporter = new ErrorReporter(DialogService);
        return await errorReporter.CanClosePageOnErrorAsync(errorMessage,
          isClosingWindow);
      }
    }
    // I'm not sure whether insisting that all errors on the page are fixed is a good
    // idea. A specific check for prerequisites is made when attempting to open
    // the GUI Script Processor page.
    // If implemented, this needs to allow for window closing, as with the
    // TrySaveSettings error message above.
    // if (GetErrors().Any()) {
    //   await DialogService.ShowErrorMessageBoxAsync(
    //     $"You must fix the error(s) on the {TabTitle} page before continuing.");
    //   return false;
    // }
    return await base.QueryCloseAsync(isClosingWindow);
  }

  private bool TrySaveSettings(out string errorMessage) {
    if (string.IsNullOrWhiteSpace(SettingsFolderPath)) {
      // Console.WriteLine(
      //   $"SettingsWriterViewModelBase.TrySaveSettings ({GetType().Name}): A settings folder has not been specified.");
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
    // if (SettingsFolderPath == SettingsFolderLocation.Path) {
    if (SettingsFolderPath == SettingsFolderLocation.Path &&
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