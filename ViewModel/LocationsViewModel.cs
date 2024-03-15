using System.Windows.Input;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public class LocationsViewModel : SettingsWriterViewModelBase {
  public LocationsViewModel() {
    BrowseForDefaultTemplateCommand = new AsyncRelayCommand(BrowseForDefaultTemplate);
    BrowseForOriginalProgramsFolderCommand = 
      new AsyncRelayCommand(BrowseForOriginalProgramsFolder);
    BrowseForProgramsFolderCommand = new AsyncRelayCommand(BrowseForProgramsFolder);
    BrowseForSettingsFolderCommand = new AsyncRelayCommand(BrowseForSettingsFolder);
    BrowseForTemplateProgramsFolderCommand = 
      new AsyncRelayCommand(BrowseForTemplateProgramsFolder);
  }

  public ICommand BrowseForDefaultTemplateCommand { get; }
  public ICommand BrowseForOriginalProgramsFolderCommand { get; }
  public ICommand BrowseForProgramsFolderCommand { get; }
  public ICommand BrowseForSettingsFolderCommand { get; }
  public ICommand BrowseForTemplateProgramsFolderCommand { get; }

  public string DefaultTemplatePath {
    get => Settings.DefaultTemplate.Path;
    set {
      if (Settings.DefaultTemplate.Path != value) {
        Settings.DefaultTemplate.Path = value;
        OnPropertyChanged();
      }
    }
  }

  public string OriginalProgramsFolderPath {
    get => Settings.OriginalProgramsFolder.Path;
    set {
      if (Settings.OriginalProgramsFolder.Path != value) {
        Settings.OriginalProgramsFolder.Path = value;
        OnPropertyChanged();
      }
    }
  }

  public string ProgramsFolderPath {
    get => Settings.ProgramsFolder.Path;
    set {
      if (Settings.ProgramsFolder.Path != value) {
        Settings.ProgramsFolder.Path = value;
        OnPropertyChanged();
      }
    }
  }

  public string TemplateProgramsFolderPath {
    get => Settings.TemplateProgramsFolder.Path;
    set {
      if (Settings.TemplateProgramsFolder.Path != value) {
        Settings.TemplateProgramsFolder.Path = value;
        OnPropertyChanged();
      }
    }
  }

  private async Task BrowseForDefaultTemplate() {
    var result = await BrowseForFalconProgramFile(
      "Select the default template Falcon program");
    if (result != null) {
      DefaultTemplatePath = result.FullPath;
    }
  }

  private async Task<FileResult?> BrowseForFalconProgramFile(string pickerTitle) {
    var fileTypesDictionary = new Dictionary<DevicePlatform, IEnumerable<string>> {
      { DevicePlatform.MacCatalyst, ["*.uvip"] }, // ???
      { DevicePlatform.WinUI, ["*.uvip"] }
    };
    var options = new PickOptions {
      FileTypes = new FilePickerFileType(fileTypesDictionary),
      PickerTitle = pickerTitle
    };
#pragma warning disable CA1416 // This call site is reachable on ...
    return await FilePicker.PickAsync(options);
  }

  private async Task<FolderPickerResult> BrowseForFolder() {
    // In Windows at least, specifying the initial path seems not to do anything.
    // FolderPicker remembers its previous initial folder regardless, which should be
    // fine.
    // string initialPath = string.Empty;
    // if (FileSystemService.FolderExists(SettingsFolderPath)) {
    //   initialPath = SettingsFolderPath;
    // }
    // return await FolderPicker.PickAsync(initialPath);
#pragma warning disable CA1416 // This call site is reachable on ...
    return await FolderPicker.PickAsync();
 }

  private async Task BrowseForOriginalProgramsFolder() {
    var result = await BrowseForFolder();
    if (result.IsSuccessful) {
      OriginalProgramsFolderPath = result.Folder.Path;
    }
  }

  private async Task BrowseForProgramsFolder() {
    var result = await BrowseForFolder();
    if (result.IsSuccessful) {
      ProgramsFolderPath = result.Folder.Path;
    }
  }

  private async Task BrowseForSettingsFolder() {
    var result = await BrowseForFolder();
    if (result.IsSuccessful) {
      SettingsFolderPath = result.Folder.Path;
    }
  }

  private async Task BrowseForTemplateProgramsFolder() {
    var result = await BrowseForFolder();
    if (result.IsSuccessful) {
      TemplateProgramsFolderPath = result.Folder.Path;
    }
  }
}