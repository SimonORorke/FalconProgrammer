namespace FalconProgrammer.Model;

public class BatchLog : IBatchLog {
  public void WriteLine(string text) {
    OnLineWritten(text);
  }

  public event EventHandler<string>? LineWritten;

  private void OnLineWritten(string text) {
    LineWritten?.Invoke(this, text);
  }
}