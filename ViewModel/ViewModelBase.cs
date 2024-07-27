using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using CommunityToolkit.Mvvm.Messaging;
using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.ViewModel;

public abstract class ViewModelBase : ObservableRecipientWithValidation {
  private IFileSystemService? _fileSystemService;
  private ModelServices? _modelServices;
  private SettingsFolderLocationReader? _settingsFolderLocationReader;
  private string _settingsFolderPath = string.Empty;
  private SettingsReader? _settingsReader;

  protected ViewModelBase(IDialogService dialogService,
    IDispatcherService dispatcherService) {
    DialogService = dialogService;
    DispatcherService = dispatcherService;
  }

  protected ICursorService? CursorService { get; set; }
  internal IDialogService DialogService { get; }
  protected IDispatcherService DispatcherService { get; }

  internal IFileSystemService FileSystemService =>
    _fileSystemService ??= ModelServices.FileSystemService;

  /// <summary>
  ///   Gets or sets whether the page is being reopened to fix one or more errors, in
  ///   which case, to ensure that the error data is still there to be fixed, when the
  ///   page is reopened, its data will not be refreshed from saved settings.
  /// </summary>
  /// <remarks>
  ///   This does not work for field errors, only for inter-field consistency errors.
  /// </remarks>
  internal bool IsFixingError { get; set; }

  protected bool IsVisible { get; private set; }

  /// <summary>
  ///   Title to be shown at the top of the main window when the page is selected and
  ///   shown.
  /// </summary>
  public abstract string PageTitle { get; }

  /// <summary>
  ///   Gets or set the path of the settings folder, where a settings will be stored in a
  ///   settings file with the invariant name 'Settings.xml'
  /// </summary>
  /// <remarks>
  ///   Specification of the settings file name is not supported because the application
  ///   saves settings automatically, without prompting the user for the file path.
  ///   The path of a file that does not yet exist cannot be specified in advance,
  ///   whereas the path of its containing folder can.
  /// </remarks>
  [Required]
  [CustomValidation(typeof(ViewModelBase),
    nameof(ValidateSettingsFolderPath))]
  public string SettingsFolderPath {
    get => _settingsFolderPath;
    set => SetProperty(ref _settingsFolderPath, value, true);
  }

  /// <summary>
  ///   Title to be shown on the page's tab. Defaults to the same as
  ///   <see cref="PageTitle" />.
  /// </summary>
  public virtual string TabTitle => PageTitle;

  internal ModelServices ModelServices {
    [ExcludeFromCodeCoverage] get => _modelServices ??= new ModelServices();
    set => _modelServices = value; // For tests
  }

  internal Settings Settings { get; private protected set; } = null!;

  protected SettingsFolderLocationReader SettingsFolderLocationReader =>
    _settingsFolderLocationReader ??= ModelServices.SettingsFolderLocationReader;

  private SettingsReader SettingsReader =>
    _settingsReader ??= ModelServices.SettingsReader;

  internal void GoToLocationsPage() {
    // using CommunityToolkit.Mvvm.Messaging is needed to provide this Send extension
    // method.
    Messenger.Send(new GoToLocationsPageMessage());
  }

  internal virtual async Task Open() {
    // Debug.WriteLine($"ViewModelBase.Open: {GetType().Name}");
    IsVisible = true;
    // Start listening for ObservableRecipient messages.
    Messenger.RegisterAll(this);
    if (IsFixingError) {
      IsFixingError = false;
      return;
    }
    await ReadSettings();
  }

  internal virtual async Task<bool> QueryClose(bool isClosingWindow = false) {
    // Debug.WriteLine($"ViewModelBase.QueryClose: {GetType().Name}");
    IsVisible = false;
    // Stop listening for ObservableRecipient messages.
    Messenger.UnregisterAll(this);
    await Task.Delay(0);
    return true;
  }

  /// <summary>
  ///   Handles an XML error in a settings file.
  /// </summary>
  protected virtual async Task OnSettingsXmlError(string errorMessage) {
    await Task.Delay(0);
    // One way the user can fix the problem is by choosing a different settings folder
    // on the Locations page. LocationsViewModel will attempt to read settings on opening
    // and get the same error, which it will report in an error message box.
    GoToLocationsPage();
  }

  protected async Task ReadSettings() {
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
      await OnSettingsXmlError(errorMessage);
    }
  }

  protected static ValidationResult ValidateFolderPath(
    string propertyName,
    string folderPath, ValidationContext context) {
    var instance = (SettingsWriterViewModelBase)context.ObjectInstance;
    bool isValid = instance.FileSystemService.Folder.Exists(folderPath);
    return isValid
      ? ValidationResult.Success!
      : new ValidationResult("Cannot find folder.", [propertyName]);
  }

  public static ValidationResult ValidateSettingsFolderPath(
    string folderPath, ValidationContext context) {
    return ValidateFolderPath(nameof(SettingsFolderPath), folderPath, context);
  }
}