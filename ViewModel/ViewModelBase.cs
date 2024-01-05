using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class ViewModelBase : ObservableObject {
  private IAlertService? _alertService;
  private IFilePicker? _filePicker;
  private IFileSystemService? _fileSystemService;
  private IFolderPicker? _folderPicker;
  private ServiceHelper? _serviceHelper;
  private Settings? _settings;

  protected IAlertService AlertService =>
    _alertService ??= ServiceHelper.GetService<IAlertService>();

  protected IFilePicker FilePicker =>
    _filePicker ??= ServiceHelper.GetService<IFilePicker>();

  protected IFileSystemService FileSystemService =>
    _fileSystemService ??= ServiceHelper.GetService<IFileSystemService>();

  protected IFolderPicker FolderPicker =>
    _folderPicker ??= ServiceHelper.GetService<IFolderPicker>();

  protected bool IsVisible { get; private set; }

  internal ServiceHelper ServiceHelper {
    get => _serviceHelper ??= ServiceHelper.Default;
    // For unit testing.
    // ReSharper disable once UnusedMember.Global
    set => _serviceHelper = value;
  }

  protected Settings Settings => _settings ??= Settings.Read();

  public virtual void OnAppearing() {
    IsVisible = true;
  }

  public virtual void OnDisappearing() {
    IsVisible = false;
  }
}