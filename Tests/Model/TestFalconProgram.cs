﻿using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

internal class TestFalconProgram : FalconProgram {
  private TestProgramXml? _testProgramXml;

  public TestFalconProgram(
    string embeddedProgramFileName,
    string path, Category category, Batch batch)
    : base(path, category, batch) {
    EmbeddedProgramFileName = embeddedProgramFileName;
  }

  internal string EmbeddedProgramFileName { get; set; }
  internal string LastWrittenFilePath { get; private set; } = string.Empty;
  internal string LastWrittenFileContents { get; private set; } = string.Empty;
  internal string SavedXml { get; private set; } = string.Empty;

  private TestProgramXml TestProgramXml {
    get {
      return _testProgramXml ??= CreateTestProgramXml();

      TestProgramXml CreateTestProgramXml() {
        var result = new TestProgramXml(Category) {
          EmbeddedProgramFileName = EmbeddedProgramFileName
        };
        result.Saved += TestProgramXmlOnSaved;
        return result;
      }
    }
  }

  protected override void CopyFile(string sourcePath, string destinationPath) { }

  protected override ProgramXml CreateProgramXml() {
    return TestProgramXml;
  }

  private void TestProgramXmlOnSaved(object? sender, string e) {
    SavedXml = e;
  }

  protected override void WriteTextToFile(string path, string contents) {
    LastWrittenFilePath = path;
    LastWrittenFileContents = contents;
  }
}