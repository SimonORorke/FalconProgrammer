using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class SettingsWriterViewModelBase(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogService, dispatcherService) {
  private SettingsFolderLocation? _settingsFolderLocation;
  private string _settingsFolderPath = string.Empty;
  private bool HaveSettingsBeenUpdated { get; set; }

  private SettingsFolderLocation SettingsFolderLocation => 
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

  internal override void Open() {
    base.Open();
    SettingsFolderPath = SettingsFolderLocation.Path;
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    if (HaveSettingsBeenUpdated) {
      if (!TrySaveSettings(out string errorMessage)) {
        if (isClosingWindow) {
          errorMessage += 
            $"\r\n\r\nAnswer Yes (Enter) to close {Global.ApplicationTitle}, " + 
            "No (Esc) to resume.";
          return await DialogService.AskYesNoQuestionAsync(errorMessage);
        }
        await DialogService.ShowErrorMessageBoxAsync(errorMessage);
        return false;
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
    return await base.QueryClose(isClosingWindow);
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
    if (SettingsFolderPath == SettingsFolderLocation.Path) {
      Settings.Write();
    } else {
      SettingsFolderLocation.Path = SettingsFolderPath;
      SettingsFolderLocation.Write();
      Settings.Write(SettingsFolderPath);
    }
    HaveSettingsBeenUpdated = false;
    return true;
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