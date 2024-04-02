﻿using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class SettingsWriterViewModelBase(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogWrapper, dispatcherService) {
  private SettingsFolderLocation? _settingsFolderLocation;
  private string? _settingsFolderPath;
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

  public string SettingsFolderPath {
    get => _settingsFolderPath ??= SettingsFolderLocation.Path;
    set {
      if (_settingsFolderPath != value) {
        _settingsFolderPath = value;
        OnPropertyChanged();
      }
    }
  }

  private bool CanSaveSettings() {
    // The message box won't show if the application is closing.
    if (string.IsNullOrWhiteSpace(SettingsFolderPath)) {
      // Console.WriteLine(
      //   $"SettingsWriterViewModelBase.CanSaveSettings ({GetType().Name}): A settings folder has not been specified.");
      DialogWrapper.ShowErrorMessageBoxAsync(this,
        "Settings cannot be saved: a settings folder has not been specified.");
      return false;
    }
    if (!FileSystemService.FolderExists(SettingsFolderPath)) {
      DialogWrapper.ShowErrorMessageBoxAsync(this,
        "Settings cannot be saved: cannot find settings folder "
        + $"'{SettingsFolderPath}'.");
      return false;
    }
    return true;
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (IsVisible) {
      HaveSettingsBeenUpdated = true;
    }
  }

  public override bool QueryClose() {
    if (HaveSettingsBeenUpdated) {
      if (CanSaveSettings()) {
        SaveSettings();
        HaveSettingsBeenUpdated = false;
      } else {
        return false;
      }
    }
    return base.QueryClose();
  }

  private void SaveSettings() {
    // Debug.WriteLine($"SettingsWriterViewModelBase.SaveSettings: {GetType().Name}");
    if (SettingsFolderPath == SettingsFolderLocation.Path) {
      Settings.Write();
    } else {
      SettingsFolderLocation.Path = SettingsFolderPath;
      SettingsFolderLocation.Write();
      Settings.Write(SettingsFolderPath);
    }
  }
}