using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class ViewModelBase(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService) : ObservableObject {
  private IFileSystemService? _fileSystemService;
  private ISerialiser? _serialiser;
  private ServiceHelper? _serviceHelper;
  private Settings? _settings;
  private SettingsReader? _settingsReader;
  protected IDialogWrapper DialogWrapper { get; } = dialogWrapper;
  protected IDispatcherService DispatcherService { get; } = dispatcherService;

  internal IFileSystemService FileSystemService =>
    // The Avalonia UI App won't be providing an IFileSystemService to ServiceHelper.
    // But tests may.
    _fileSystemService ??= ServiceHelper.GetService<IFileSystemService>() ??
                           Model.FileSystemService.Default;

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
    // The Avalonia UI App won't be providing an ISerialiser to ServiceHelper.
    // But tests may.
    _serialiser ??= ServiceHelper.GetService<ISerialiser>() ??
                    Model.Serialiser.Default;

  public ServiceHelper ServiceHelper {
    [ExcludeFromCodeCoverage] get => _serviceHelper ??= ServiceHelper.Default;
    // For unit testing.
    internal set => _serviceHelper = value;
  }

  internal Settings Settings {
    get => _settings ??= ReadSettings();
    private set => _settings = value;
  }

  private SettingsReader SettingsReader =>
    // The Avalonia UI App won't be providing a SettingsReader to ServiceHelper.
    // But tests may.
    _settingsReader ??= ServiceHelper.GetService<SettingsReader>() ??
                        new SettingsReader();

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