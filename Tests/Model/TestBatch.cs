using FalconProgrammer.Model;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.Model;

public class TestBatch : Batch {
  public TestBatch(IBatchLog? log = null) : base(log ?? new MockBatchLog()) {
    FileSystemService = MockFileSystemService = new MockFileSystemService();
    SettingsReader = TestSettingsReaderEmbedded = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "LocationsSettings.xml"
    };
    BatchScriptReader =
      TestBatchScriptReaderEmbedded = new TestBatchScriptReaderEmbedded();
  }

  internal string EmbeddedProgramFileName { get; set; } = "NoGuiScriptProcessor.uvip";

  [PublicAPI]
  internal string EmbeddedScriptFileName { get; set; } =
    "QueriesForProgram.xml";

  internal string EmbeddedTemplateFileName { get; set; } = "NoGuiScriptProcessor.uvip";
  internal bool HasScriptRunEnded { get; private set; }
  internal MockBatchLog MockBatchLog => (MockBatchLog)Log;
  internal MockFileSystemService MockFileSystemService { get; }
  private TestBatchScriptReaderEmbedded TestBatchScriptReaderEmbedded { get; }
  internal TestFalconProgram TestProgram => (TestFalconProgram)Program;
  internal TestSettingsReaderEmbedded TestSettingsReaderEmbedded { get; }
  internal Exception? ExceptionWhenConfiguringProgram { get; set; }

  /// <summary>
  ///   If false, modification of Falcon programs will be bypassed and instead
  ///   details of each bypassed task will be logged. Default: true.
  /// </summary>
  internal bool UpdatePrograms { get; set; } = true;

  protected override void ConfigureProgram() {
    if (ExceptionWhenConfiguringProgram != null) {
      throw ExceptionWhenConfiguringProgram;
    }
    if (UpdatePrograms) {
      base.ConfigureProgram();
      return;
    }
    Log.WriteLine($"{Task}: '{Program.PathShort}'");
  }

  protected override Category CreateCategory(string categoryName) {
    var result = new TestCategory(SoundBankFolderPath, categoryName, Settings);
    if (MockFileSystemService.Folder.SimulatedFilePaths.TryGetValue(
          result.Path, out var
            categorySimulatedFilePaths)) {
      result.MockFileSystemService.Folder.SimulatedFilePaths.Add(result.Path,
        categorySimulatedFilePaths);
    }
    result.EmbeddedTemplateFileName = EmbeddedTemplateFileName;
    result.Initialise();
    return result;
  }

  protected override FalconProgram CreateFalconProgram(string path) {
    return new TestFalconProgram(
      EmbeddedProgramFileName, EmbeddedTemplateFileName, path, Category, this);
  }

  protected override void OnScriptRunEnded() {
    base.OnScriptRunEnded();
    HasScriptRunEnded = true;
  }

  public override void RunScript(
    string batchScriptPath, CancellationToken cancellationToken) {
    HasScriptRunEnded = false;
    TestBatchScriptReaderEmbedded.EmbeddedFileName = EmbeddedScriptFileName;
    base.RunScript(batchScriptPath, cancellationToken);
  }
}