using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class TestBatch : Batch {
  public TestBatch() : base(new MockBatchLog()) {
    FileSystemService = MockFileSystemService = new MockFileSystemService();
    SettingsReader = TestSettingsReaderEmbedded = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "LocationsSettings.xml"
    };
    BatchScriptReader =
      TestBatchScriptReaderEmbedded = new TestBatchScriptReaderEmbedded();
  }

  internal MockBatchLog MockBatchLog => (MockBatchLog)Log;
  internal MockFileSystemService MockFileSystemService { get; }
  internal TestBatchScriptReaderEmbedded TestBatchScriptReaderEmbedded { get; }
  internal TestFalconProgram TestProgram => (TestFalconProgram)Program;
  internal TestSettingsReaderEmbedded TestSettingsReaderEmbedded { get; }

  /// <summary>
  ///   If false, modification of Falcon programs will be bypassed and instead
  ///   details of each bypassed task will be logged. Default: true.
  /// </summary>
  internal bool UpdatePrograms { get; set; } = true;

  protected override void ConfigureProgram() {
    if (UpdatePrograms) {
      base.ConfigureProgram();
      return;
    }
    Log.WriteLine($"{Task}: '{Program.PathShort}'");
  }

  protected override Category CreateCategory(string categoryName) {
    var result = new TestCategory(SoundBankFolderPath, categoryName, Settings);
    if (MockFileSystemService.Folder.ExpectedFilePaths.TryGetValue(
          result.Path, out var
            categoryExpectedFilePaths)) {
      result.MockFileSystemService.Folder.ExpectedFilePaths.Add(result.Path,
        categoryExpectedFilePaths);
    }
    if (result.MustUseGuiScriptProcessor) {
      result.EmbeddedTemplateFileName = "GuiScriptProcessor.uvip";
    }
    result.Initialise();
    return result;
  }

  protected override FalconProgram CreateFalconProgram(string path) {
    return new TestFalconProgram(path, Category, this);
  }
}