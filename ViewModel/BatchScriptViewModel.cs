using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class BatchScriptViewModel : SettingsWriterViewModelBase {
  private Batch? _batch;
  private BatchLog? _batchLog;

  private BatchScopeCollection? _scopes;
  private string _status = string.Empty;
  private TaskCollection? _tasks;

  public BatchScriptViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  protected Batch Batch => _batch ??= CreateInitialisedBatch();
  internal BatchLog BatchLog => _batchLog ??= CreateBatchLog();
  private bool IsRunStarting { get; set; }
  public ObservableCollection<string> Log { get; } = [];
  private ConcurrentQueue<string> LogLineQueue { get; } = [];
  [ExcludeFromCodeCoverage] public override string PageTitle => "Run a batch Script";

  protected CancellationTokenSource RunCancellationTokenSource { get; private set; } =
    new CancellationTokenSource();

  [ExcludeFromCodeCoverage] private DateTime RunCurrentTime { get; set; }
  private DateTime RunStartTime { get; set; }

  internal ProgramItem Scope => Scopes[0];

  public BatchScopeCollection Scopes => _scopes
    ??= new BatchScopeCollection(FileSystemService, DispatcherService);

  private ImmutableList<string> SoundBanks { get; set; } = [];

  public string Status {
    get => _status;
    private set => SetProperty(ref _status, value);
  }

  public override string TabTitle => "Batch Script";

  public TaskCollection Tasks => _tasks ??= new TaskCollection(DispatcherService);
  public event EventHandler<string>? CopyToClipboard;
  public event EventHandler? LogUpdated;
  public event EventHandler? RunBeginning;
  public event EventHandler? RunEnded;

  private void BatchLogOnLineWritten(object? sender, string text) {
    LogLineQueue.Enqueue(text);
    if (IsRunStarting) {
      // Provide an initial progress update, showing that the first task is running.
      IsRunStarting = false;
      DispatcherService.Dispatch(UpdateLogAndProgress);
      return;
    }
    // We need to give the GUI opportunities to repaint when necessary and allow the
    // batch run to be cancelled. So, once every 10th of a second, pause the batch thread
    // for a millisecond and update the displayed log. This is over 10 times faster than
    // pausing the batch thread and updating the log every time a new log line had been
    // produced. Making the periodic pause and update once per second sped up the run
    // slightly: 50 seconds compared with 55 seconds for a a roll forward of all
    // programs, for example. But every 10th of a second makes it look like it is going
    // faster!
    var currentTime = DateTime.Now;
    if (IsTimeToUpdateLogAndProgress(currentTime, 100)) {
      RunCurrentTime = currentTime;
      Thread.Sleep(1);
      DispatcherService.Dispatch(UpdateLogAndProgress);
    }
  }

  private void BatchOnScriptRunEnded(object? sender, EventArgs e) {
    RunCancellationTokenSource.Dispose(); // Stops next run cancelling immediately.
    DispatcherService.Dispatch(() => {
      // The displayed log has only been updating once every 10th of a second.
      // So there will almost certainly be some lines still to be displayed.
      UpdateLogAndProgress();
      OnRunEnded();
      var runEndTime = DateTime.Now;
      var runDuration = runEndTime - RunStartTime;
      Status = $"Run ended at {runEndTime:HH:mm:ss}. Duration: {runDuration:g}.";
    });
  }

  private async Task<string?> BrowseForBatchScriptFile(string purpose) {
    return await DialogService.OpenFile(
      $"Select a batch script file to {purpose}",
      "XML files", "xml");
  }

  /// <summary>
  ///   Generates <see cref="CancelRunCommand" />.
  /// </summary>
  [RelayCommand]
  private void CancelRun() {
    RunCancellationTokenSource.Cancel();
  }

  /// <summary>
  ///   Generates <see cref="CopyLogCommand" />.
  /// </summary>
  [RelayCommand]
  private void CopyLog() {
    OnCopyToClipboard(GetLogText());
    Status = "Copied log to clipboard.";
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
  ///   Generates <see cref="EditScriptFileCommand" />.
  /// </summary>
  [RelayCommand]
  [ExcludeFromCodeCoverage]
  protected virtual async Task EditScriptFile() {
    string? path = await BrowseForBatchScriptFile("edit");
    if (path != null) {
      Process.Start(new ProcessStartInfo(path) {
        UseShellExecute = true
      });
    }
  }

  private string GetLogText() {
    var writer = new StringWriter();
    foreach (string line in Log) {
      writer.WriteLine(line);
    }
    return writer.ToString();
  }

  [ExcludeFromCodeCoverage]
  protected virtual bool IsTimeToUpdateLogAndProgress(
    DateTime currentTime, int intervalMilliseconds) {
    return currentTime - RunCurrentTime >=
           TimeSpan.FromMilliseconds(intervalMilliseconds);
  }

  /// <summary>
  ///   Generates <see cref="LoadScriptCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task LoadScript() {
    string? path = await BrowseForBatchScriptFile("load");
    if (path != null) {
      BatchScript script;
      try {
        script = Batch.ReadScript(path);
      } catch (XmlException exception) {
        await DialogService.ShowErrorMessageBox(exception.Message, TabTitle);
        return;
      }
      Scopes.LoadFromScript(script);
      Tasks.LoadFromScript(script);
      Status = $"Loaded script '{Path.GetFileName(path)}'.";
    }
  }

  protected virtual void OnCopyToClipboard(string text) {
    CopyToClipboard?.Invoke(this, text);
  }

  private void OnLogUpdated() {
    LogUpdated?.Invoke(this, EventArgs.Empty);
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
    IsRunStarting = true;
    // I thought re-instantiating the log might be quicker than clearing a long log.
    // But I don't think it makes any difference.
    Log.Clear();
    // Needs to be refreshed each time, in case the previous run was cancelled.
    RunCancellationTokenSource = new CancellationTokenSource();
    RunCurrentTime = RunStartTime = DateTime.Now;
    Status = $"Run started at {RunStartTime:HH:mm:ss}.";
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
    // If the user goes to another page when a long log is shown, there can be several
    // seconds delay when later returning to this Batch Script page. So clear the log on
    // closing this page.
    // I thought re-instantiating the log might be quicker than clearing a long log.
    // But I don't think it makes any difference.
    Log.Clear();
    Status = string.Empty;
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }

  /// <summary>
  ///   Generates <see cref="RunScriptCommand" />.
  /// </summary>
  [RelayCommand]
  private void RunScript() {
    var script = CreateInitialisedBatchScript();
    PrepareForRun();
    StartThread(() => Batch.RunScript(script, RunCancellationTokenSource.Token),
      nameof(RunScript));
  }

  /// <summary>
  ///   Generates <see cref="SaveScriptCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task SaveScript() {
    string? path = await DialogService.SaveFile(
      "Save Batch Script", "XML files", "xml");
    if (path != null) {
      var script = CreateInitialisedBatchScript();
      script.Serialiser.Serialise(script, path);
      Status = $"Saved script to '{Path.GetFileName(path)}'.";
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

  private void UpdateLogAndProgress() {
    while (!LogLineQueue.IsEmpty) {
      if (LogLineQueue.TryDequeue(out string? line)) {
        Log.Add(line);
      }
    }
    Status = $"Run started at {RunStartTime:HH:mm:ss}. Running {Batch.Task}, " +
             $"task {Batch.TaskNo} of {Batch.TaskCount}.";
    OnLogUpdated();
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
    return originalSoundBanks.Count != 0;
  }
}