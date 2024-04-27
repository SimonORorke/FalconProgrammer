namespace FalconProgrammer.Model;

public class ConsoleBatchLog : IBatchLog {
  public void WriteLine(string text) {
    Console.WriteLine(text);
  }
}