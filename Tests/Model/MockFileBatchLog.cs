using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   Maybe not needed.
/// </summary>
[ExcludeFromCodeCoverage]
public class MockFileBatchLog : FileBatchLog {
  public MockFileBatchLog(string path) : base(path) { }
  protected override TextWriter Writer => new StringWriter();
  internal string SavedText { get; private set; } = string.Empty;

  public override void Save() {
    SavedText = ToString();
    base.Save();
  }

  public override string ToString() {
    return Writer.ToString() ?? string.Empty;
    ;
  }
}