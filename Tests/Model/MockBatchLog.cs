using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockBatchLog : IBatchLog {
  internal string Text => ToString();
  internal List<string> Lines { get; } = [];

  public override string ToString() {
    using var writer = new StringWriter();
    foreach (string line in Lines) {
      writer.WriteLine(line);
    }
    return writer.ToString();
  }

  public void WriteLine(string text) {
    Lines.Add(text);
  }
}