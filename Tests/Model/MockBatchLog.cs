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

  public async Task WriteLine(string text) {
    await Task.Delay(0);
    Lines.Add(text);
  }
}