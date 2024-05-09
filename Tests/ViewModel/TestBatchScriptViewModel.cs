using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class TestBatchScriptViewModel : BatchScriptViewModel {
  public TestBatchScriptViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  internal string SavedLog { get; private set; } = string.Empty;

  [ExcludeFromCodeCoverage]
  [PublicAPI]
  internal TestBatch TestBatch => (TestBatch)Batch;
  
  internal TestBatchScript? TestBatchScript { get; private set; }

  protected override Batch CreateBatch() {
    return new TestBatch(BatchLog) { UpdatePrograms = false };
  }

  protected override BatchScript CreateThisBatchScript() {
    TestBatchScript = new TestBatchScript {
      Tasks = CreateBatchTasks()
    };
    return TestBatchScript;
  }

  [ExcludeFromCodeCoverage]
  protected override async Task EditSavedScript() {
    await Task.Delay(0);
  }

  protected override void SaveLogToFile(string outputPath) {
    SavedLog = BatchLog.ToString();
  }
}