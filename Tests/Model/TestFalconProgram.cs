using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

public class TestFalconProgram : FalconProgram {
  private TestProgramXml? _testProgramXml;

  public TestFalconProgram(
    string embeddedProgramFileName, string embeddedTemplateFileName, 
    string path, Category category, Batch batch)
    : base(path, category, batch) {
    EmbeddedProgramFileName = embeddedProgramFileName;
    EmbeddedTemplateFileName = embeddedTemplateFileName;
  }

  private string EmbeddedProgramFileName { get; }
  private string EmbeddedTemplateFileName { get; }
  internal string SavedXml { get; set; } = string.Empty;

  internal TestProgramXml TestProgramXml {
    get {
      return _testProgramXml ??= CreateTestProgramXml();

      TestProgramXml CreateTestProgramXml() {
        var result = new TestProgramXml(Category) {
          EmbeddedProgramFileName = EmbeddedProgramFileName,
          EmbeddedTemplateFileName = EmbeddedTemplateFileName
        };
        result.Saved += TestProgramXmlOnSaved;
        return result;
      }
    }
  }

  /// <summary>
  ///   Only used for Organic Pads sound bank in <see cref="FalconProgram.FixCData" />.
  /// </summary>
  protected override TextReader CreateProgramReader() {
    return new StringReader(SavedXml);
  }

  protected override ProgramXml CreateProgramXml() {
    return TestProgramXml;
  }

  private void TestProgramXmlOnSaved(object? sender, string e) {
    SavedXml = e;
  }

  /// <summary>
  ///   Only used for Organic Pads sound bank in <see cref="FalconProgram.FixCData" />.
  /// </summary>
  protected override void UpdateProgramFileWithFixedCData(string newContents) {
    SavedXml = newContents;
  }
}