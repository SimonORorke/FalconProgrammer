using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract partial class SettingsWriterViewModelBase(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogService, dispatcherService) {
  private SettingsFolderLocation? _settingsFolderLocation;

  /// <summary>
  ///   Generates the SettingsFolderPath property.
  /// </summary>
  [ObservableProperty] private string _settingsFolderPath = string.Empty;

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

  private bool CanSaveSettings() {
    // The message box won't show if the application is closing.
    if (string.IsNullOrWhiteSpace(SettingsFolderPath)) {
      // Console.WriteLine(
      //   $"SettingsWriterViewModelBase.CanSaveSettings ({GetType().Name}): A settings folder has not been specified.");
      DialogService.ShowErrorMessageBoxAsync(
        "Settings cannot be saved: a settings folder has not been specified.");
      return false;
    }
    if (!FileSystemService.Folder.Exists(SettingsFolderPath)) {
      DialogService.ShowErrorMessageBoxAsync(
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

  public override void Open() {
    base.Open();
    SettingsFolderPath = SettingsFolderLocation.Path;
  }

  public override bool QueryClose() {
    if (HaveSettingsBeenUpdated) {
      if (CanSaveSettings()) {
        SaveSettings();
        // try {
        //   SaveSettings();
        // } catch (IOException) {
        //   Console.WriteLine($"SettingsWriterViewModelBase.QueryClose {GetType().Name}: SaveSettings throwing IOException.");
        //   throw;
        // }
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
}