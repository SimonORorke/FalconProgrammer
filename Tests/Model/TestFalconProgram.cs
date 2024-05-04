using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

public class TestFalconProgram : FalconProgram {
  private TestProgramXml? _testProgramXml;

  public TestFalconProgram(string path, Category category, Batch batch) : base(path,
    category, batch) { }

  internal TestProgramXml TestProgramXml => 
    _testProgramXml ??= new TestProgramXml(Category);

  protected override ProgramXml CreateProgramXml() {
    return TestProgramXml;
  }
}