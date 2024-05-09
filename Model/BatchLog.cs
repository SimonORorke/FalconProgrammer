namespace FalconProgrammer.Model;

public class BatchLog : IBatchLog {

  public BatchLog(Func<string, Task> onWriteLine) {
    OnWriteLine = onWriteLine;
  }

  private Func<string, Task> OnWriteLine { get; }
  
  public async Task WriteLine(string text) {
    await OnWriteLine(text);
  }
}