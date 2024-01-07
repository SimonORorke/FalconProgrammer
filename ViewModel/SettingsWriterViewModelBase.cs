using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class SettingsWriterViewModelBase : ViewModelBase {
  private SettingsFolderLocation? _settingsFolderLocation;
  private string? _settingsFolderPath;
  private bool HaveSettingsBeenUpdated { get; set; }

  private SettingsFolderLocation SettingsFolderLocation {
    [SuppressMessage("ReSharper", "CommentTypo")]
    get {
      if (_settingsFolderLocation == null) {
        // SettingsFolderLocation.AppDataFolderPathMaui = 
        //   FileSystemService.AppDataFolderPathMaui;
        Debug.WriteLine("====================================================");
        Debug.WriteLine(
          "SettingsFolderLocation.AppDataFolderPathMaui = " +
          $"'{SettingsFolderLocation.AppDataFolderPathMaui}'");
        // C:\Users\Simon O'Rorke\AppData\Local\Packages\com.simonororke.falconprogrammer_9zz4h110yvjzm\LocalState
        Debug.WriteLine("====================================================");
        _settingsFolderLocation =
          SettingsFolderLocation.Read(FileSystemService, Serializer);
      }
      return _settingsFolderLocation;
    }
  }

  public string SettingsFolderPath {
    get {
      if (_settingsFolderPath == null) {
        SettingsFolderLocation.AppDataFolderPathMaui =
          AppDataFolderService.AppDataFolderPathMaui;
        _settingsFolderPath = SettingsFolderLocation.Path;
      }
      return _settingsFolderPath;
    }
    set {
      if (_settingsFolderPath != value) {
        _settingsFolderPath = value;
        OnPropertyChanged();
      }
    }
  }

  private bool CanSaveSettings() {
    // The alerts won't show if the application is closing.
    if (string.IsNullOrWhiteSpace(SettingsFolderPath)) {
      AlertService.ShowAlert("Error",
        "Settings cannot be saved: a settings folder has not been specified.");
      return false;
    }
    if (!FileSystemService.FolderExists(SettingsFolderPath)) {
      AlertService.ShowAlert("Error",
        "Settings cannot be saved: cannot find settings folder "
        + $"'{SettingsFolderPath}'.");
      return false;
    }
    return true;
  }

  public override void OnDisappearing() {
    base.OnDisappearing();
    if (HaveSettingsBeenUpdated && CanSaveSettings()) {
      SaveSettings();
      HaveSettingsBeenUpdated = false;
    }
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (IsVisible) {
      HaveSettingsBeenUpdated = true;
      // We don't want to show setting folder error messages if the user is in the
      // process of specifying the settings folder.
      if (e.PropertyName != nameof(SettingsFolderPath)) {
        // We just want to see any settings folder error message box at this stage.
        bool dummy = CanSaveSettings();
      }
    }
  }

  private void SaveSettings() {
    if (SettingsFolderPath == SettingsFolderLocation.Path) {
      Settings.Write();
    } else {
      SettingsFolderLocation.Path = SettingsFolderPath;
      SettingsFolderLocation.Write();
      Settings.Write(SettingsFolderPath);
    }
  }
}