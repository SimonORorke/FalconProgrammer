using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.Model;

/// <summary>
///   Maybe not needed.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileBatchLog : IBatchLog {
  private TextWriter? _writer;

  public FileBatchLog(string path) {
    Path = path;
  }

  [ExcludeFromCodeCoverage]
  protected virtual TextWriter Writer => _writer ??= new StreamWriter(Path);

  public string Path { get; }

  public void WriteLine(string text) {
    Writer.WriteLine(text);
  }

  public virtual void Save() {
    Writer.Close();
  }
}