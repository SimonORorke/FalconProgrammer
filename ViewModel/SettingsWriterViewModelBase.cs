using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class SettingsWriterViewModelBase : ViewModelBase {
  private SettingsFolderLocation? _settingsFolderLocation;
  private string? _settingsFolderPath = string.Empty;
  
  private bool HaveSettingsBeenUpdated { get; set; }

  private SettingsFolderLocation SettingsFolderLocation {
    [SuppressMessage("ReSharper", "CommentTypo")]
    get {
      if (_settingsFolderLocation == null) {
        SettingsFolderLocation.AppDataFolderPathMaui = 
          FileSystemService.AppDataFolderPathMaui;
        Debug.WriteLine("====================================================");
        Debug.WriteLine(
          "SettingsFolderLocation.AppDataFolderPathMaui = " + 
          $"'{SettingsFolderLocation.AppDataFolderPathMaui}'");
        // C:\Users\Simon O'Rorke\AppData\Local\Packages\com.simonororke.falconprogrammer_9zz4h110yvjzm\LocalState
        Debug.WriteLine("====================================================");
        _settingsFolderLocation = SettingsFolderLocation.Read();
      }
      return _settingsFolderLocation;
    }
  }

  public string SettingsFolderPath {
    get => _settingsFolderPath ??= SettingsFolderLocation.Path;
    set {
      if (_settingsFolderPath != value) {
        _settingsFolderPath = value;
        OnPropertyChanged();
      }
    }
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (IsVisible) {
      HaveSettingsBeenUpdated = true;
      if (string.IsNullOrEmpty(SettingsFolderPath)) {
        AlertService.ShowAlert("Error", 
          "Settings cannot be saved: a settings folder has not been specified.");
        return;
      }
      if (!FileSystemService.FolderExists(SettingsFolderPath)) {
        AlertService.ShowAlert("Error", 
          "Settings cannot be saved: cannot find settings folder '"
          + $"'{SettingsFolderPath}'.");
      }
    }
  }

  public override void OnDisappearing() {
    base.OnDisappearing();
    if (HaveSettingsBeenUpdated) {
      SaveSettings();
      HaveSettingsBeenUpdated = false;
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