using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Messaging;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class ViewModelBase(
  IDialogService dialogService,
  IDispatcherService dispatcherService) : ObservableRecipientWithValidation {
  private IFileSystemService? _fileSystemService;
  private ModelServices? _modelServices;
  private SettingsFolderLocationReader? _settingsFolderLocationReader;
  private SettingsReader? _settingsReader;
  protected IDialogService DialogService { get; } = dialogService;
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

  internal ModelServices ModelServices {
    [ExcludeFromCodeCoverage] get => _modelServices ??= ModelServices.Default;
    // For unit testing.
    set => _modelServices = value;
  }

  internal Settings Settings { get; private set; } = null!;

  protected SettingsFolderLocationReader SettingsFolderLocationReader =>
    _settingsFolderLocationReader ??=
      ModelServices.GetService<SettingsFolderLocationReader>();

  private SettingsReader SettingsReader =>
    _settingsReader ??= ModelServices.GetService<SettingsReader>();

  protected void GoToLocationsPage() {
    // using CommunityToolkit.Mvvm.Messaging is needed to provide this Send extension
    // method.
    Messenger.Send(new GoToLocationsPageMessage());
  }

  internal virtual void Open() {
    // Debug.WriteLine($"ViewModelBase.Open: {GetType().Name}");
    IsVisible = true;
    // Start listening for ObservableRecipient messages.
    Messenger.RegisterAll(this);
    Settings = SettingsReader.Read(true);
  }

  internal virtual async Task<bool> QueryCloseAsync(bool isClosingWindow = false) {
    // Debug.WriteLine($"ViewModelBase.QueryCloseAsync: {GetType().Name}");
    IsVisible = false;
    // Stop listening for ObservableRecipient messages.
    Messenger.UnregisterAll(this);
    await Task.Delay(0);
    return true;
  }
}