namespace FalconProgrammer.Model;

public class SubscribableBatchLog : IBatchLog {
  public List<string> Lines { get; } = [];

  public event EventHandler? LineWritten;

  private void OnLineWritten() {
    LineWritten?.Invoke(this, EventArgs.Empty);
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
    OnLineWritten();
  }
}