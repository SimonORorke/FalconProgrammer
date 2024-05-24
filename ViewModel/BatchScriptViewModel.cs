using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class BatchScriptViewModel : SettingsWriterViewModelBase {
  private Batch? _batch;
  private BatchLog? _batchLog;
  private BatchScopeCollection? _scopes;
  private TaskCollection? _tasks;

  public BatchScriptViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  protected Batch Batch => _batch ??= CreateInitialisedBatch();
  internal BatchLog BatchLog => _batchLog ??= CreateBatchLog();

  public ObservableCollection<string> Log { get; } = [];

  [ExcludeFromCodeCoverage] public override string PageTitle => "Run a batch Script";

  protected CancellationTokenSource RunCancellationTokenSource { get; private set; } =
    new CancellationTokenSource();

  internal ProgramItem Scope => Scopes[0];

  public BatchScopeCollection Scopes => _scopes
    ??= new BatchScopeCollection(FileSystemService, DispatcherService);

  private ImmutableList<string> SoundBanks { get; set; } = [];
  public override string TabTitle => "Batch Script";

  public TaskCollection Tasks => _tasks ??= new TaskCollection(DispatcherService);
  public event EventHandler? LogLineWritten;
  public event EventHandler? RunBeginning;
  public event EventHandler? RunEnded;

  private void BatchLogOnLineWritten(object? sender, string text) {
    // The batch thread needs to tak a breather to give the GUI an opportunity to repaint
    // when necessary and to allow the batch to be cancelled.
    Thread.Sleep(1);
    DispatcherService.Dispatch(() => {
      Log.Add(text);
      LogLineWritten?.Invoke(this, EventArgs.Empty);
    });
  }

  private void BatchOnScriptRunEnded(object? sender, EventArgs e) {
    RunCancellationTokenSource.Dispose();
    DispatcherService.Dispatch(OnRunEnded);
  }

  private async Task<string?> BrowseForBatchScriptFile(string purpose) {
    return await DialogService.OpenFile(
      $"Open a batch script file to {purpose}",
      "XML files", "xml");
  }

  /// <summary>
  ///   Generates <see cref="CancelBatchRunCommand" />.
  /// </summary>
  [RelayCommand]
  private void CancelBatchRun() {
    RunCancellationTokenSource.Cancel();
  }

  [ExcludeFromCodeCoverage]
  protected virtual Batch CreateBatch() {
    return new Batch(BatchLog);
  }

  private BatchLog CreateBatchLog() {
    var result = new BatchLog();
    result.LineWritten += BatchLogOnLineWritten;
    return result;
  }

  [ExcludeFromCodeCoverage]
  protected virtual BatchScript CreateBatchScript() {
    return new BatchScript();
  }

  private Batch CreateInitialisedBatch() {
    var result = CreateBatch();
    result.ScriptRunEnded += BatchOnScriptRunEnded;
    return result;
  }

  private BatchScript CreateInitialisedBatchScript() {
    var result = CreateBatchScript();
    result.Scope.SoundBank = Scope.SoundBank;
    result.Scope.Category = Scope.Category;
    result.Scope.Program = Scope.Program;
    result.Tasks = (
      from task in Tasks
      where !task.IsAdditionItem
      select task.Name).ToList();
    return result;
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

  private void OnRunBeginning() {
    RunBeginning?.Invoke(this, EventArgs.Empty);
  }

  private void OnRunEnded() {
    RunEnded?.Invoke(this, EventArgs.Empty);
  }

  internal override async Task Open() {
    await base.Open();
    if (!await ValidateAndPopulateSoundBanks()) {
      return;
    }
    Scopes.Populate(Settings, SoundBanks);
    Tasks.Populate(Settings);
  }

  protected virtual void PrepareForRun() {
    Log.Clear();
    RunCancellationTokenSource = new CancellationTokenSource();
    OnRunBeginning();
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
  [RelayCommand]
  private async Task RunSavedScript() {
    string? path = await BrowseForBatchScriptFile("run");
    if (path != null) {
      PrepareForRun();
      StartThread(() => Batch.RunScript(path, RunCancellationTokenSource.Token),
        nameof(RunSavedScript));
    }
  }

  /// <summary>
  ///   Generates <see cref="RunThisScriptCommand" />.
  /// </summary>
  [RelayCommand]
  private void RunThisScript() {
    var script = CreateInitialisedBatchScript();
    PrepareForRun();
    StartThread(() => Batch.RunScript(script, RunCancellationTokenSource.Token),
      nameof(RunThisScript));
  }

  /// <summary>
  ///   Generates <see cref="SaveLogCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task SaveLog() {
    string? path = await DialogService.SaveFile(
      "Save Log", "Text files", "txt");
    if (path != null) {
      SaveLogToFile(path);
    }
  }

  [ExcludeFromCodeCoverage]
  protected virtual void SaveLogToFile(string outputPath) {
    File.WriteAllText(outputPath, BatchLog.ToString());
  }

  /// <summary>
  ///   Generates <see cref="SaveThisScriptCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task SaveThisScript() {
    string? path = await DialogService.SaveFile(
      "Save Batch Script", "XML files", "xml");
    if (path != null) {
      var script = CreateInitialisedBatchScript();
      script.Serialiser.Serialise(script, path);
    }
  }

  [ExcludeFromCodeCoverage]
  protected virtual void StartThread(Action action, string threadName) {
    var thread =
      new Thread(action.Invoke) {
        Name = threadName
      };
    thread.Start();
  }

  private async Task<bool> ValidateAndPopulateSoundBanks() {
    var validator = new SettingsValidator(this,
      "All folder/file locations must be " +
      "specified in the settings." + Environment.NewLine + Environment.NewLine +
      "Batch scripts cannot be run", TabTitle);
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