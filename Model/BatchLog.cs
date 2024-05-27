namespace FalconProgrammer.Model;

public class BatchLog : IBatchLog {
  public string Prefix { get; set; } = string.Empty;

  public void WriteLine(string text) {
    OnLineWritten($"{Prefix}{text}");
  }

  public event EventHandler<string>? LineWritten;

  private void OnLineWritten(string text) {
    LineWritten?.Invoke(this, text);
  }
}