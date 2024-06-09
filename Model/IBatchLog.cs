namespace FalconProgrammer.Model;

public interface IBatchLog {
  string Prefix { get; set; }
  void WriteLine(string text);
}