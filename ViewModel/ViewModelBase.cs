using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class ViewModelBase : ObservableObject {
  private IAlertService? _alertService;
  private IAppDataFolderService? _appDataFolderService;
  private IFilePicker? _filePicker;
  private IFileSystemService? _fileSystemService;
  private IFolderPicker? _folderPicker;
  private ISerialiser? _serialiser;
  private ServiceHelper? _serviceHelper;
  private Settings? _settings;

  internal IAlertService AlertService =>
    _alertService ??=
      ServiceHelper.GetService<IAlertService>()
      ?? throw new InvalidOperationException(
        "ServiceHelper does not have an IAlertService");

  internal IAppDataFolderService AppDataFolderService =>
    _appDataFolderService ??=
      ServiceHelper.GetService<IAppDataFolderService>()
      ?? throw new InvalidOperationException(
        "ServiceHelper does not have an IAppDataFolderService");

  internal IFilePicker FilePicker =>
    _filePicker ??= ServiceHelper.GetService<IFilePicker>()
                    ?? throw new InvalidOperationException(
                      "ServiceHelper does not have an IFilePicker");

  internal IFileSystemService FileSystemService =>
    // The MauiProgram won't be providing an IFileSystemService to ServiceHelper.
    // But tests may.
    _fileSystemService ??= ServiceHelper.GetService<IFileSystemService>() ??
                           Model.FileSystemService.Default;

  protected IFolderPicker FolderPicker =>
    _folderPicker ??= ServiceHelper.GetService<IFolderPicker>()
                      ?? throw new InvalidOperationException(
                        "ServiceHelper does not have an IFilePicker");

  protected bool IsVisible { get; private set; }

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  protected ISerialiser Serialiser =>
    // The MauiProgram won't be providing an ISerialiser to ServiceHelper.
    // But tests may.
    _serialiser ??= ServiceHelper.GetService<ISerialiser>() ??
                    Model.Serialiser.Default;

  internal ServiceHelper ServiceHelper {
    [ExcludeFromCodeCoverage] get => _serviceHelper ??= ServiceHelper.Default;
    // For unit testing.
    set => _serviceHelper = value;
  }

  protected Settings Settings => 
    _settings ??= Settings.Read(FileSystemService, Serialiser);

  public virtual void OnAppearing() {
    IsVisible = true;
  }

  public virtual void OnDisappearing() {
    IsVisible = false;
  }
}