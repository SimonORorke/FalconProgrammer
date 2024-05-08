using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class BatchScriptViewModel : SettingsWriterViewModelBase {
  private Batch? _batch;
  private bool _canCancelBatchRun;
  private bool _canRunSavedScript = true;
  private bool _canRunThisScript = true;
  private bool _canSaveLog = true;
  private string _log = string.Empty;
  private BatchScopeCollection? _scopes;
  private TaskCollection? _tasks;

  public BatchScriptViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) {
    BatchLog = new SubscribableBatchLog();
    BatchLog.LineWritten += BatchLogOnLineWritten;
  }

  protected Batch Batch {
    get {
      if (_batch == null) {
        _batch = CreateBatch();
        _batch.ScriptRunEnded += BatchOnScriptRunEnded;
        
      }
      return _batch;
    }
  }

  internal SubscribableBatchLog BatchLog { get; }

  /// <summary>
  ///   Gets or sets CanExecute for <see cref="CancelBatchRunCommand" />.
  /// </summary>
  public bool CanCancelBatchRun {
    get => _canCancelBatchRun;
    private set => SetProperty(ref _canCancelBatchRun, value);
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
  ///   Gets or sets CanExecute for <see cref="SaveLogCommand" />.
  /// </summary>
  public bool CanSaveLog {
    get => _canSaveLog;
    private set => SetProperty(ref _canSaveLog, value);
  }

  internal string Log {
    get => _log;
    private set => SetProperty(ref _log, value);
  }

  [ExcludeFromCodeCoverage] public override string PageTitle => "Run a batch Script";

  private CancellationTokenSource RunCancellationTokenSource { get; } =
    new CancellationTokenSource();

  internal BatchScope Scope => Scopes[0]; 

  public BatchScopeCollection Scopes => _scopes
    ??= new BatchScopeCollection(FileSystemService, DispatcherService);

  private ImmutableList<string> SoundBanks { get; set; } = [];
  public override string TabTitle => "Batch Script";

  public TaskCollection Tasks => _tasks ??= new TaskCollection(DispatcherService);

  private void BatchLogOnLineWritten(object? sender, EventArgs e) {
    DispatcherService.Dispatch(() => Log = BatchLog.ToString());
  }

  private void BatchOnScriptRunEnded(object? sender, EventArgs e) {
    CanSaveLog = CanRunSavedScript = CanRunThisScript = true;
    CanCancelBatchRun = false;
  }

  private async Task<string?> BrowseForBatchScriptFile(string purpose) {
    return await DialogService.OpenFile(
      $"Open a batch script file to {purpose}",
      "XML files", "xml");
  }

  /// <summary>
  ///   Generates <see cref="CancelBatchRunCommand" />.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanCancelBatchRun))]
  private void CancelBatchRun() {
    RunCancellationTokenSource.Cancel();
  }

  [ExcludeFromCodeCoverage]
  protected virtual Batch CreateBatch() {
    return new Batch(BatchLog);
  }

  protected List<BatchScript.BatchTask> CreateBatchTasks() {
    return (
      from task in Tasks
      where !task.IsAdditionItem
      select new BatchScript.BatchTask {
        Name = task.Name,
        SoundBank = Scope.SoundBank,
        Category = Scope.Category,
        Program = Scope.Program
      }).ToList();
  }

  [ExcludeFromCodeCoverage]
  protected virtual BatchScript CreateThisBatchScript() {
    return new BatchScript {
      Tasks = CreateBatchTasks()
    };
  }

  /// <summary>
  ///   Generates <see cref="EditSavedScriptCommand" />.
  /// </summary>
  [RelayCommand]
  [ExcludeFromCodeCoverage]
  protected virtual async Task EditSavedScript() {
    string? path = await BrowseForBatchScriptFile("edit");
    if (path != null) {
      Process.Start(new ProcessStartInfo(path) {
        UseShellExecute = true
      });
    }
  }

  internal override async Task Open() {
    await base.Open();
    if (!await ValidateAndPopulateSoundBanks()) {
      return;
    }
    Scopes.Populate(Settings, SoundBanks);
    Tasks.Populate(Settings);
  }

  private void PrepareForRun() {
    BatchLog.Lines.Clear();
    Log = string.Empty;
    CanSaveLog = CanRunSavedScript = CanRunThisScript = false;
    CanCancelBatchRun = true;
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

  /// <summary>
  ///   Generates <see cref="RunSavedScriptCommand" />.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanRunSavedScript))]
  private async Task RunSavedScript() {
    string? path = await BrowseForBatchScriptFile("run");
    if (path != null) {
      PrepareForRun();
      await Batch.RunScript(path, RunCancellationTokenSource.Token);
    }
  }

  /// <summary>
  ///   Generates <see cref="RunThisScriptCommand" />.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanRunThisScript))]
  private async Task RunThisScript() {
    var script = CreateThisBatchScript();
    PrepareForRun();
    await Batch.RunScript(script, RunCancellationTokenSource.Token);
  }

  /// <summary>
  ///   Generates <see cref="SaveLogCommand" />.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanSaveLog))]
  private async Task SaveLog() {
    string? path = await DialogService.SaveFile(
      "Save Log", "txt");
    if (path != null) {
      SaveLogToFile(path);
    }
  }


  [ExcludeFromCodeCoverage]
  protected virtual void SaveLogToFile(string outputPath) {
    File.WriteAllText(outputPath, Log);
  }

  /// <summary>
  ///   Generates <see cref="SaveThisScriptCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task SaveThisScript() {
    string? path = await DialogService.SaveFile(
      "Save Batch Script", "xml");
    if (path != null) {
      var script = CreateThisBatchScript();
      script.Serialiser.Serialise(typeof(BatchScript), script, path);
    }
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