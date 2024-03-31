using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class ViewModelBase(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService) : ObservableObject {
  private IFileSystemService? _fileSystemService;
  private ISerialiser? _serialiser;
  private ModelServices? _modelServices;
  private Settings? _settings;
  private SettingsReader? _settingsReader;
  protected IDialogWrapper DialogWrapper { get; } = dialogWrapper;
  protected IDispatcherService DispatcherService { get; } = dispatcherService;

  internal IFileSystemService FileSystemService =>
    _fileSystemService ??= ModelServices.GetService<IFileSystemService>();

  protected bool IsVisible { get; private set; }

  /// <summary>
  ///   Title to be shown at the top of the main window when the page is selected and
  ///   shown.
  /// </summary>
  public abstract string PageTitle { get; }

  /// <summary>
  ///   Title to be shown on the page's tab. Defaults to the same as
  ///   <see cref="PageTitle" />.
  /// </summary>
  public virtual string TabTitle => PageTitle;

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  protected ISerialiser Serialiser =>
    _serialiser ??= ModelServices.GetService<ISerialiser>();

  internal ModelServices ModelServices {
    [ExcludeFromCodeCoverage] get => _modelServices ??= ModelServices.Default;
    // For unit testing.
    set => _modelServices = value;
  }

  internal Settings Settings {
    get => _settings ??= ReadSettings();
    private set => _settings = value;
  }

  private SettingsReader SettingsReader =>
    _settingsReader ??= ModelServices.GetService<SettingsReader>(); 

  public IContentPageBase View { get; set; } = null!;

  protected virtual void Initialise() {
    Settings = ReadSettings();
  }

  public virtual void OnAppearing() {
    // Debug.WriteLine($"ViewModelBase.OnAppearing: {GetType().Name}");
    IsVisible = true;
    // Reads Settings on the Dispatcher thread so that any updates to Settings made on
    // the previous page will be available on this one.
    DispatcherService.Dispatch(Initialise);
    // new Thread(Initialise).Start(); // Does not work, needs Dispatcher.
  }

  public virtual void OnDisappearing() {
    // Debug.WriteLine($"ViewModelBase.OnDisappearing: {GetType().Name}");
    IsVisible = false;
  }

  private Settings ReadSettings() {
    // Debug.WriteLine($"ViewModelBase.ReadSettings: {GetType().Name}");
    return SettingsReader.Read(true);
  }
}