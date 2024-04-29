using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockBatchLog : IBatchLog {
  private StringWriter Writer { get; } = new StringWriter();
  public void WriteLine(string text) {
    Writer.WriteLine(text);
  }

  public override string ToString() {
    return Writer.ToString() ?? string.Empty;;
  }
}