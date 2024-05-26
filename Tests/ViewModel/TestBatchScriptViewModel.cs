using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class TestBatchScriptViewModel : BatchScriptViewModel {
  public TestBatchScriptViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  internal string SavedLog { get; private set; } = string.Empty;

  [ExcludeFromCodeCoverage] [PublicAPI] internal TestBatch TestBatch => (TestBatch)Batch;

  internal TestBatchScript? TestBatchScript { get; private set; }

  protected override Batch CreateBatch() {
    return new TestBatch(BatchLog) { UpdatePrograms = false };
  }

  protected override BatchScript CreateBatchScript() {
    return TestBatchScript = new TestBatchScript();
  }

  [ExcludeFromCodeCoverage]
  protected override async Task EditScriptFile() {
    await Task.Delay(0);
  }

  protected override bool IsTimeToUpdateLogAndProgress(
    DateTime currentTime, int intervalMilliseconds) {
    return true;
  }

  protected override void PrepareForRun() {
    bool isCancelling = RunCancellationTokenSource.IsCancellationRequested;
    base.PrepareForRun();
    // RunCancellationTokenSource will have been replaced with a new instance.
    // But we want to test cancelling before any tasks have been run.
    if (isCancelling) {
      // CancelBatchRunCommand was executed before RunThisScriptCommand.
      RunCancellationTokenSource.Cancel();
    }
  }

  protected override void SaveLogToFile(string outputPath) {
    SavedLog = GetLogText();
  }

  /// <summary>
  ///   For unknown reason, testing a run of a saved script on a separate thread, which
  ///   was working fine, started to sometimes fail to finish and time out. So we mock
  ///   out running batches on a separate thread.
  /// </summary>
  protected override void StartThread(Action action, string threadName) {
    action.Invoke();
  }
}