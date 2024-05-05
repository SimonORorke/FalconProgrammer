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

  internal TestProgramXml TestProgramXml =>
    _testProgramXml ??= new TestProgramXml(Category) {
      EmbeddedProgramFileName = EmbeddedProgramFileName,
      EmbeddedTemplateFileName = EmbeddedTemplateFileName
    };

  /// <summary>
  ///   Only used for Organic Pads sound bank in <see cref="FalconProgram.FixCData" />.
  /// </summary>
  protected override TextReader CreateProgramReader() {
    return new StringReader(TestProgramXml.SavedXml);
  }

  protected override ProgramXml CreateProgramXml() {
    return TestProgramXml;
  }

  /// <summary>
  ///   Only used for Organic Pads sound bank in <see cref="FalconProgram.FixCData" />.
  /// </summary>
  protected override void UpdateProgramFileWithFixedCData(string newContents) {
    TestProgramXml.SavedXml = newContents;
  }
}