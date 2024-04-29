using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.Model;

public class TestBatch : Batch {
  public TestBatch() : base(new MockBatchLog()) {
    FileSystemService = MockFileSystemService = new MockFileSystemService();
    SettingsReader = TestSettingsReaderEmbedded = new TestSettingsReaderEmbedded {
      TestDeserialiser = {
        EmbeddedFileName = "LocationsSettings.xml"
      }
    };
    BatchScriptReader =
      TestBatchScriptReaderEmbedded = new TestBatchScriptReaderEmbedded();
  }

  internal MockBatchLog MockBatchLog => (MockBatchLog)Log;
  
  [ExcludeFromCodeCoverage, PublicAPI] 
  internal MockFileSystemService MockFileSystemService { get; }
  internal TestBatchScriptReaderEmbedded TestBatchScriptReaderEmbedded { get; }
  
  [ExcludeFromCodeCoverage, PublicAPI] 
  internal TestSettingsReaderEmbedded TestSettingsReaderEmbedded { get; }

  protected override void ConfigureProgram() {
    Log.WriteLine($"{Task}: '{Program.PathShort}'");
  }

  protected override Category CreateCategory(string categoryName) {
    var result = new TestCategory(SoundBankFolderPath, categoryName, Settings);
    result.Initialise();
    return result;
  }
}