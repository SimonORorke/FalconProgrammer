using System.Diagnostics.CodeAnalysis;
using System.Xml;
using CommunityToolkit.Mvvm.Messaging;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class ViewModelBase : ObservableRecipientWithValidation {
  private IFileSystemService? _fileSystemService;
  private ModelServices? _modelServices;
  private SettingsFolderLocationReader? _settingsFolderLocationReader;
  private SettingsReader? _settingsReader;

  protected ViewModelBase(IDialogService dialogService,
    IDispatcherService dispatcherService) {
    DialogService = dialogService;
    DispatcherService = dispatcherService;
  }

  protected IDialogService DialogService { get; }
  protected IDispatcherService DispatcherService { get; }

  internal IFileSystemService FileSystemService =>
    _fileSystemService ??= ModelServices.FileSystemService;

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
    [ExcludeFromCodeCoverage] get => _modelServices ??= new ModelServices();
    set => _modelServices = value; // For tests
  }

  internal Settings Settings { get; private set; } = null!;

  protected SettingsFolderLocationReader SettingsFolderLocationReader =>
    _settingsFolderLocationReader ??= ModelServices.SettingsFolderLocationReader;

  private SettingsReader SettingsReader =>
    _settingsReader ??= ModelServices.SettingsReader;

  protected void GoToLocationsPage() {
    // using CommunityToolkit.Mvvm.Messaging is needed to provide this Send extension
    // method.
    Messenger.Send(new GoToLocationsPageMessage());
  }

  internal virtual async Task Open() {
    // Debug.WriteLine($"ViewModelBase.Open: {GetType().Name}");
    IsVisible = true;
    // Start listening for ObservableRecipient messages.
    Messenger.RegisterAll(this);
    await ReadSettingsAsync();
  }

  internal virtual async Task<bool> QueryCloseAsync(bool isClosingWindow = false) {
    // Debug.WriteLine($"ViewModelBase.QueryCloseAsync: {GetType().Name}");
    IsVisible = false;
    // Stop listening for ObservableRecipient messages.
    Messenger.UnregisterAll(this);
    await Task.Delay(0);
    return true;
  }

  /// <summary>
  ///   Handles an XML error in a settings file.
  /// </summary>
  protected virtual async Task OnSettingsXmlErrorAsync(string errorMessage) {
    await Task.Delay(0);
    // One way the user can fix the problem is by choosing a different settings folder
    // on the Locations page. LocationsViewModel will attempt to read settings on opening
    // and get the same error, which it will report in an error message box.
    GoToLocationsPage();
  }

  protected async Task ReadSettingsAsync() {
    try {
      Settings = SettingsReader.Read(true);
    } catch (XmlException exception) {
      Settings = new Settings();
      // This must be an error in the settings file, not the settings folder locations
      // file. See the comment in SettingsFolderLocationReader.Read.
      // So make the error message "Invalid XML was found in '{inputPath}'." more
      // specific.
      string errorMessage =
        exception.Message.Replace(" in '", " in settings file '");
      await OnSettingsXmlErrorAsync(errorMessage);
    }
  }
}