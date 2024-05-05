using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockBatchLog : IBatchLog {
  internal List<string> Lines { get; } = [];

  internal string Text => GetText();

  private string GetText() {
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