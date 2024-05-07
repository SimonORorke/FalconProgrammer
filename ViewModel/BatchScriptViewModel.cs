using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public partial class BatchScriptViewModel : SettingsWriterViewModelBase {
  private bool _canEditSavedScript;
  private bool _canRunSavedScript;
  private bool _canRunThisScript;
  private bool _canSaveThisScript;
  private string _log = string.Empty;
  private BatchScopeCollection? _scopes;
  private TaskCollection? _tasks;

  public BatchScriptViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  /// <summary>
  ///   Gets or sets CanExecute for <see cref="EditSavedScriptCommand" />.
  /// </summary>
  public bool CanEditSavedScript {
    get => _canEditSavedScript;
    private set => SetProperty(ref _canEditSavedScript, value);
  }

  /// <summary>
  ///   Gets or sets CanExecute for <see cref="RunSavedScriptCommand" />.
  /// </summary>
  public bool CanRunSavedScript {
    get => _canRunSavedScript;
    private set => SetProperty(ref _canRunSavedScript, value);
  }

  /// <summary>
  ///   Gets or sets CanExecute for <see cref="RunThisScriptCommand" />.
  /// </summary>
  public bool CanRunThisScript {
    get => _canRunThisScript;
    private set => SetProperty(ref _canRunThisScript, value);
  }

  /// <summary>
  ///   Gets or sets CanExecute for <see cref="SaveThisScriptCommand" />.
  /// </summary>
  public bool CanSaveThisScript {
    get => _canSaveThisScript;
    private set => SetProperty(ref _canSaveThisScript, value);
  }
  public string Log {
    get => _log;
    private set => SetProperty(ref _log, value);
  }

  [ExcludeFromCodeCoverage] public override string PageTitle => "Run a batch Script";

  public BatchScopeCollection Scopes => _scopes
    ??= new BatchScopeCollection(FileSystemService, DispatcherService);

  private ImmutableList<string> SoundBanks { get; set; } = [];
  public override string TabTitle => "Batch Script";

  public TaskCollection Tasks => _tasks ??= new TaskCollection(DispatcherService);

  /// <summary>
  ///   Generates <see cref="EditSavedScriptCommand" />.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanEditSavedScript))]
  private void EditSavedScript() {
  }

  /// <summary>
  ///   Generates <see cref="RunSavedScriptCommand" />.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanRunSavedScript))]
  private void RunSavedScript() {
  }

  /// <summary>
  ///   Generates <see cref="RunThisScriptCommand" />.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanRunThisScript))]
  private void RunThisScript() {
  }

  /// <summary>
  ///   Generates <see cref="SaveThisScriptCommand" />.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanSaveThisScript))]
  private void SaveThisScript() {
  }

  internal override async Task Open() {
    await base.Open();
    if (!await ValidateAndPopulateSoundBanks()) {
      return;
    }
    Scopes.Populate(Settings, SoundBanks);
    Tasks.Populate(Settings);
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    if (Scopes.HasBeenChanged) {
      Scopes.UpdateSettings();
    }
    if (Tasks.HasBeenChanged) {
      Tasks.UpdateSettings();
    }
    if (Scopes.HasBeenChanged || Tasks.HasBeenChanged) {
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }

  private async Task<bool> ValidateAndPopulateSoundBanks() {
    var validator = new SettingsValidator(this,
      "All folder/file locations must be " +
      "specified in the settings." + Environment.NewLine + Environment.NewLine +
      "Batch scripts cannot be run");
    SoundBanks =
      await validator.GetProgramsFolderSoundBankNames();
    if (SoundBanks.Count == 0) {
      return false;
    }
    var originalSoundBanks =
      await validator.GetOriginalProgramsFolderSoundBankNames();
    if (originalSoundBanks.Count == 0) {
      return false;
    }
    var templateSoundBanks =
      await validator.GetTemplateProgramsFolderSoundBankNames();
    if (templateSoundBanks.Count == 0) {
      return false;
    }
    return await validator.ValidateDefaultTemplateFile();
  }
}