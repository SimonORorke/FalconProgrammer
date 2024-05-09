namespace FalconProgrammer.Model;

public class SubscribableBatchLog : IBatchLog {
  public List<string> Lines { get; } = [];

  public event EventHandler<string>? LineWritten;

  private void OnLineWritten(string text) {
    LineWritten?.Invoke(this, text);
  }

  public override string ToString() {
    using var writer = new StringWriter();
    foreach (string line in Lines) {
      writer.WriteLine(line);
    }
    return writer.ToString();
  }

  public void WriteLine(string text) {
    Lines.Add(text);
    OnLineWritten(text);
  }
}