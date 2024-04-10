using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class SettingsWriterViewModelBase(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogService, dispatcherService) {
  private SettingsFolderLocation? _settingsFolderLocation;
  private string _settingsFolderPath = string.Empty;
  private bool HaveSettingsBeenUpdated { get; set; }

  private SettingsFolderLocation SettingsFolderLocation {
    [SuppressMessage("ReSharper", "CommentTypo")]
    get {
      if (_settingsFolderLocation == null) {
        var settingsFolderLocationReader = new SettingsFolderLocationReader {
          FileSystemService = FileSystemService,
          Serialiser = Serialiser
        };
        _settingsFolderLocation =
          settingsFolderLocationReader.Read();
      }
      return _settingsFolderLocation;
    }
  }

  [Required]
  [CustomValidation(typeof(SettingsWriterViewModelBase),
    nameof(ValidateSettingsFolderPath))]
  public string SettingsFolderPath {
    get => _settingsFolderPath;
    set => SetProperty(ref _settingsFolderPath, value, true);
  }

  private bool CanSaveSettings(out string errorMessage) {
    if (string.IsNullOrWhiteSpace(SettingsFolderPath)) {
      // Console.WriteLine(
      //   $"SettingsWriterViewModelBase.CanSaveSettings ({GetType().Name}): A settings folder has not been specified.");
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
    return true;
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (IsVisible) {
      HaveSettingsBeenUpdated = true;
    }
  }

  public override void Open() {
    base.Open();
    SettingsFolderPath = SettingsFolderLocation.Path;
  }

  public override async Task<bool> QueryClose(bool isClosingWindow = false) {
    if (HaveSettingsBeenUpdated) {
      if (CanSaveSettings(out string errorMessage)) {
        SaveSettings();
        HaveSettingsBeenUpdated = false;
      } else if (isClosingWindow) {
        errorMessage += 
          $"\r\n\r\nClick Yes to close {Global.ApplicationTitle}, No to resume.";
        return await DialogService.AskYesNoQuestionAsync(errorMessage);
      } else {
        await DialogService.ShowErrorMessageBoxAsync(errorMessage);
        return false;
      }
    }
    // if (GetErrors().Any()) {
    //   await DialogService.ShowErrorMessageBoxAsync(
    //     $"You must fix the error(s) on the {TabTitle} page before continuing.");
    //   return false;
    // }
    return await base.QueryClose(isClosingWindow);
  }

  private void SaveSettings() {
    // Debug.WriteLine($"SettingsWriterViewModelBase.SaveSettings: {GetType().Name}");
    if (SettingsFolderPath == SettingsFolderLocation.Path) {
      Settings.Write();
      // try {
      //   Settings.Write();
      // } catch (IOException) {
      //   Console.WriteLine($"SettingsWriterViewModelBase.SaveSettings {GetType().Name}: Settings.Write throwing IOException.");
      //   throw;
      // }
    } else {
      SettingsFolderLocation.Path = SettingsFolderPath;
      SettingsFolderLocation.Write();
      Settings.Write(SettingsFolderPath);
    }
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