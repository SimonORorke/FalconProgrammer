using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

public class TestFalconProgram : FalconProgram {
  private string _savedXml = string.Empty;
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

  internal string SavedXml {
    get {
      if (ProgramTextWriter != null) {
        _savedXml = ProgramTextWriter.ToString();
      }
      return _savedXml;
    }
    private set => _savedXml = value;
  }

  private TestProgramXml TestProgramXml {
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

  protected override void CopyFile(string sourcePath, string destinationPath) {
  }

  protected override TextReader CreateProgramTextReader() {
    return new StringReader(SavedXml);
  }

  private StringWriter? ProgramTextWriter { get; set; }

  protected override TextWriter CreateProgramTextWriter() {
    return ProgramTextWriter = new StringWriter();
  }

  protected override ProgramXml CreateProgramXml() {
    return TestProgramXml;
  }

  private void TestProgramXmlOnSaved(object? sender, string e) {
    SavedXml = e;
  }
}