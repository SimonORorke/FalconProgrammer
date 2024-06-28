using System.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class SettingsWriterViewModelBase : ViewModelBase {
  private ISettingsFolderLocation? _settingsFolderLocation;

  protected SettingsWriterViewModelBase(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  protected bool FlagSettingsUpdateOnPropertyChanged { get; set; } = true;
  protected bool HaveSettingsBeenUpdated { get; set; }

  private ISettingsFolderLocation SettingsFolderLocation =>
    _settingsFolderLocation ??= SettingsFolderLocationReader.Read();

  protected async Task<bool> CanClosePageOnError(
    bool isClosingWindow, bool askOnChangingTabs) {
    var errors = GetErrors();
    int errorCount = errors.Count();
    string errorMessage = errorCount > 1
      ? $"There are {errorCount} validation errors on the {TabTitle} page."
      : $"There is a validation error on the {TabTitle} page.";
    var errorReporter = new ErrorReporter(DialogService);
    return await errorReporter.CanClosePageOnError(
      errorMessage, TabTitle, isClosingWindow, askOnChangingTabs);
  }

  private async Task<bool> CanClosePageOnError(
    string errorMessage, bool isClosingWindow) {
    var errorReporter = new ErrorReporter(DialogService);
    bool canClosePage = await errorReporter.CanClosePageOnError(
      errorMessage, TabTitle, isClosingWindow, false);
    return canClosePage;
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (IsVisible
        // We don't want to indicate that settings need to be saved when a new settings
        // file has just been read.
        && e.PropertyName != nameof(SettingsFolderPath)
        && FlagSettingsUpdateOnPropertyChanged) {
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
        GoToLocationsPage();
        bool canClosePage = await CanClosePageOnError(errorMessage, isClosingWindow);
        if (!canClosePage) {
          return false;
        }
      }
    }
    if (HasErrors) {
      if (!await CanClosePageOnError(isClosingWindow, false)) {
        return false;
      }
    }
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
}