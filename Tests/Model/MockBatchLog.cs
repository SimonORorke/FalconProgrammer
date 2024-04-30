using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockBatchLog : IBatchLog {
  internal List<string> Lines { get; } = [];

  public void WriteLine(string text) {
    Lines.Add(text);
  }
}