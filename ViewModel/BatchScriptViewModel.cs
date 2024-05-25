﻿using System.Collections.Concurrent;
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
  public ObservableCollection<string> Log { get; } = [];
  private ConcurrentQueue<string> LogLineQueue { get; } = [];
  [ExcludeFromCodeCoverage] public override string PageTitle => "Run a batch Script";

  protected CancellationTokenSource RunCancellationTokenSource { get; private set; } =
    new CancellationTokenSource();

  private DateTime RunCurrentTime { get; set; }
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
  public event EventHandler? LogUpdated;
  public event EventHandler? RunBeginning;
  public event EventHandler? RunEnded;

  private void BatchLogOnLineWritten(object? sender, string text) {
    LogLineQueue.Enqueue(text);
    // We need to give the GUI opportunities to repaint when necessary and allow the
    // batch run to be cancelled. So, once every 10th of a second, pause the batch thread
    // for a millisecond and update the displayed log. This is over 10 times faster than
    // pausing the batch thread and updating the log every time a new log line had been
    // produced. Making the periodic pause and update once per second sped up the run
    // slightly: 50 seconds compared with 55 seconds for a a roll forward of all
    // programs, for example. But every 10th of a second makes it look like it is going
    // faster!
    var currentTime = DateTime.Now;
    if (currentTime - RunCurrentTime >= TimeSpan.FromMilliseconds(100)) {
      RunCurrentTime = currentTime;
      Thread.Sleep(1);
      DispatcherService.Dispatch(UpdateLog);
    }
  }

  private void BatchOnScriptRunEnded(object? sender, EventArgs e) {
    RunCancellationTokenSource.Dispose(); // Stops next run cancelling immediately.
    DispatcherService.Dispatch(() => {
      // The displayed log has only been updating once every 10th of a second.
      // So there will almost certainly be some lines still to be displayed.
      UpdateLog(); 
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
    }
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
    Log.Clear();
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
  ///   Generates <see cref="SaveScriptCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task SaveScript() {
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

  private void UpdateLog() {
    while (!LogLineQueue.IsEmpty) {
      if (LogLineQueue.TryDequeue(out string? line)) {
        Log.Add(line);  
      }
    }
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